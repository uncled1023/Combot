using Combot.IRCServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Combot.Modules.Plugins
{
    public class Relay : Module
    {
        public override void Initialize()
        {
            Bot.CommandReceivedEvent += HandleCommandEvent;
        }

        public override void ParseCommand(CommandMessage command)
        {
            Command foundCommand = Commands.Find(c => c.Triggers.Contains(command.Command));
            switch (foundCommand.Name)
            {
                case "Relay":
                    string method = command.Arguments["Method"];
                    switch (method.ToLower())
                    {
                        case "add":
                            AddRelay(command);
                            break;
                        case "edit":
                            EditRelay(command);
                            break;
                        case "delete":
                        case "del":
                            DeleteRelay(command);
                            break;
                        case "view":
                            ViewRelay(command);
                            break;
                    }
                    break;
            }
        }

        private void AddRelay(CommandMessage command)
        {
            string source = command.Arguments.ContainsKey("Source") ? command.Arguments["Source"] : command.Location;
            string target = command.Arguments.ContainsKey("Target") ? command.Arguments["Target"] : command.Nick.Nickname;
            string type = command.Arguments.ContainsKey("Type") ? command.Arguments["Type"] : "Message";
            string modes = command.Arguments.ContainsKey("Modes") ? command.Arguments["Modes"] : string.Empty;

            // verify access in source and target
            if (!CheckAccess(source, command.Nick.Nickname, command.Access))
            {
                string invalid = string.Format("You do not have permission to use '{0}' as a source.", source);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid, true);
                return;
            }
            if (!CheckAccess(target, command.Nick.Nickname, command.Access))
            {
                string invalid = string.Format("You do not have permission to use '{0}' as a target.", source);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid, true);
                return;
            }

            RelayType relayType = RelayType.Message;
            Enum.TryParse(type, out relayType);

            AddNick(command.Nick);
            string query = "INSERT INTO `relays` SET " +
                            "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                            "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {1} && `nickname` = {2}), " +
                            "`source` = {3}, " +
                            "`target` = {4}, " +
                            "`type` = {5}, " +
                            "`modes` = {6}, " +
                            "`date_added` = {7}";
            Bot.Database.Execute(query, new object[] { Bot.ServerConfig.Name, Bot.ServerConfig.Name, command.Nick.Nickname, source, target, (int)relayType, modes, command.TimeStamp });
            List<Dictionary<string, object>> results = GetRelayList(command.Nick.Nickname);
            string relayMessage = string.Format("Added relay from \u0002{0}\u0002 to \u0002{1}\u0002.  You now have \u0002{2}\u0002 relays created.", source, target, results.Count);
            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, relayMessage);
        }

        private void EditRelay(CommandMessage command)
        {
            string source = command.Arguments.ContainsKey("Source") ? command.Arguments["Source"] : command.Location;
            string target = command.Arguments.ContainsKey("Target") ? command.Arguments["Target"] : command.Nick.Nickname;
            string type = command.Arguments.ContainsKey("Type") ? command.Arguments["Type"] : "Message";
            string modes = command.Arguments.ContainsKey("Modes") ? command.Arguments["Modes"] : string.Empty;

            // verify access in source and target
            if (!CheckAccess(source, command.Nick.Nickname, command.Access))
            {
                string invalid = string.Format("You do not have permission to use '{0}' as a source.", source);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid, true);
                return;
            }
            if (!CheckAccess(target, command.Nick.Nickname, command.Access))
            {
                string invalid = string.Format("You do not have permission to use '{0}' as a target.", source);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid, true);
                return;
            }

            RelayType relayType = RelayType.Message;
            Enum.TryParse(type, out relayType);

            int num = HasValidID(command);

            if (num > 0)
            {
                List<Dictionary<string, object>> results = GetRelayList(command.Nick.Nickname);
                int id = Convert.ToInt32(results[num - 1]["id"]);
                string query = "UPDATE `relays` SET " +
                                "`source` = {0}, " +
                                "`target` = {1}, " +
                                "`type` = {2}, " +
                                "`modes` = {3} " +
                                "WHERE `id` = {4}";
                Bot.Database.Execute(query, new object[] { source, target, (int)relayType, modes, id });
                string relayMessage = string.Format("Updated relay \u0002{0}\u0002 to be from \u0002{1}\u0002 to \u0002{2}\u0002.", num, source, target);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, relayMessage);
            }
            else
            {
                string invalid = "Invalid relay ID.";
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid, true);
            }
        }

        private void DeleteRelay(CommandMessage command)
        {
            int num = HasValidID(command);

            if (num > 0)
            {
                List<Dictionary<string, object>> results = GetRelayList(command.Nick.Nickname);
                int id = Convert.ToInt32(results[num - 1]["id"]);
                string query = "DELETE FROM `relays` " +
                                "WHERE `id` = {0}";
                Bot.Database.Execute(query, new object[] { id });
                string relayMessage = string.Format("Relay #\u0002{0}\u0002 has been deleted.", num);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, relayMessage);
            }
            else
            {
                string invalid = "Invalid relay ID.";
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid, true);
            }
        }

        private void ViewRelay(CommandMessage command)
        {
            List<Dictionary<string, object>> results = GetRelayList(command.Nick.Nickname);

            if (command.Arguments.ContainsKey("ID"))
            {
                int num = HasValidID(command);
                if (num > 0)
                {
                    int relayInt = 0;
                    Int32.TryParse(results[num - 1]["type"].ToString(), out relayInt);

                    string relayMessage = string.Format("Relay #\u0002{0}\u0002 - Source: \u0002{1}\u0002 | Target: \u0002{2}\u0002 | Type: \u0002{3}\u0002", 
                                                        num, results[num - 1]["source"], results[num - 1]["target"], (RelayType)relayInt);
                    if ((RelayType)relayInt == RelayType.Mode)
                    {
                        relayMessage = string.Format("{0} | Modes: \u0002{1}\u0002", relayMessage, results[num - 1]["modes"]);
                    }
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, relayMessage);
                }
                else
                {
                    string invalid = "Invalid relay ID.";
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid, true);
                }
            }
            else
            {
                if (results.Any())
                {
                    for (int i = 0; i < results.Count; i++)
                    {
                        int relayInt = 0;
                        Int32.TryParse(results[i]["type"].ToString(), out relayInt);

                        string relayMessage = string.Format("Relay #\u0002{0}\u0002 - Source: \u0002{1}\u0002 | Target: \u0002{2}\u0002 | Type: \u0002{3}\u0002",
                                                            i + 1, results[i]["source"], results[i]["target"], (RelayType)relayInt);
                        if ((RelayType)relayInt == RelayType.Mode)
                        {
                            relayMessage = string.Format("{0} | Modes: \u0002{1}\u0002", relayMessage, results[i]["modes"]);
                        }
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, relayMessage, true);
                    }
                }
                else
                {
                    string invalid = "You do not have any relays set.";
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid, true);
                }
            }
        }

        private List<Dictionary<string, object>> GetRelayList(string nickname)
        {
            // Check to see if they have reached the max number of introductions
            string search = "SELECT `relays`.`id`, `relays`.`source`, `relays`.`target`, `relays`.`type`, `relays`.`modes` FROM `relays` " +
                            "INNER JOIN `nicks` " +
                            "ON `relays`.`nick_id` = `nicks`.`id` " +
                            "INNER JOIN `servers` " +
                            "ON `nicks`.`server_id` = `servers`.`id` " +
                            "WHERE `servers`.`name` = {0} AND `nicks`.`nickname` = {1}";
            return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, nickname });
        }

        private bool CheckAccess(string source, string nick, List<AccessType> types)
        {
            // Owners get to have all the fun
            if (Bot.ServerConfig.Owners.Contains(nick))
                return true;

            // The source is a channel
            if (Bot.IRC.Channels.Exists(chan => chan.Name == source))
            {
                foreach (AccessType access in types)
                {
                    bool valid = Bot.CheckChannelAccess(source, nick, AccessType.Operator);
                    if (!valid)
                        return false;
                }
            }
            // The source is a nickname
            else
            {
                if (source != nick)
                    return false;
            }
            return true;
        }

        /* Returns the parsed ID field if valid, otherwise returns 0 */
        private int HasValidID(CommandMessage command)
        {
            int num = 0;
            int ret = 0;
            List<Dictionary<string, object>> results = GetRelayList(command.Nick.Nickname);

            if (int.TryParse(command.Arguments["ID"], out num))
            {
                if (results.Count >= num && num > 0)
                {
                    ret = num;
                }
            }

            return ret;
        }
    }
}
