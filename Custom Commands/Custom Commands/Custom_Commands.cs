using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text.RegularExpressions;
using Combot.IRCServices;
using Combot.IRCServices.Messaging;

namespace Combot.Modules.Plugins
{
    public class Custom_Commands : Module
    {
        public override void Initialize()
        {
            Bot.CommandReceivedEvent += HandleCommandEvent;
            Bot.IRC.Message.ChannelMessageReceivedEvent += HandleChannelMessage;
            Bot.IRC.Message.PrivateMessageReceivedEvent += HandlePrivateMessage;
            Bot.IRC.Message.ChannelNoticeReceivedEvent += HandleChannelNotice;
            Bot.IRC.Message.PrivateNoticeReceivedEvent += HandlePrivateNotice;
        }

        public override void ParseCommand(CommandMessage command)
        {
            Command foundCommand = Commands.Find(c => c.Triggers.Contains(command.Command));
            switch (foundCommand.Name)
            {
                case "Custom Command":
                    string action = command.Arguments["Action"];
                    switch (action.ToLower())
                    {
                        case "add":
                            string addType = command.Arguments["Type"];
                            string addPermission = command.Arguments["Permission"];
                            string addChannels = (command.Arguments.ContainsKey("Channels")) ? command.Arguments["Channels"] : string.Empty;
                            string addNicknames = (command.Arguments.ContainsKey("Nicknames")) ? command.Arguments["Nicknames"] : string.Empty;
                            string addTrigger = command.Arguments["Trigger"];
                            string addResponse = command.Arguments["Response"];
                            AddCommand(command, addType, addPermission, addChannels, addNicknames, addTrigger, addResponse);
                            break;
                        case "del":
                            DeleteCommand(command);
                            break;
                        case "edit":
                            string editType = command.Arguments["Type"];
                            string editPermission = command.Arguments["Permission"];
                            string editChannels = (command.Arguments.ContainsKey("Channels")) ? command.Arguments["Channels"] : string.Empty;
                            string editNicknames = (command.Arguments.ContainsKey("Nicknames")) ? command.Arguments["Nicknames"] : string.Empty;
                            string editTrigger = command.Arguments["Trigger"];
                            string editResponse = command.Arguments["Response"];
                            EditCommand(command, editType, editPermission, editChannels, editNicknames, editTrigger, editResponse);
                            break;
                        case "view":
                            if (command.Arguments.ContainsKey("Trigger"))
                            {
                                ViewTrigger(command, command.Arguments["trigger"]);
                            }
                            else
                            {
                                ViewTriggers(command);
                            }
                            break;
                    }
                    break;
            }
        }

        private void HandleChannelMessage(object sender, ChannelMessage message)
        {
            if (Enabled && !Bot.IsCommand(message.Message))
            {
                string command = Bot.GetCommand(message.Message);
                if (!string.IsNullOrEmpty(command))
                {
                    List<Dictionary<string, object>> foundTriggers = GetTrigger(message.Sender, null, message.Channel, message.Sender.Nickname, command);
                    if (foundTriggers.Any())
                    {
                        foreach (Dictionary<string, object> foundTrigger in foundTriggers)
                        {
                            ExecuteCommand(MessageType.Channel, message.Channel, message.Sender, foundTrigger);
                        }
                    }
                }
            }
        }

        private void HandlePrivateMessage(object sender, PrivateMessage message)
        {
            if (Enabled && !Bot.IsCommand(message.Message))
            {
                string command = Bot.GetCommand(message.Message);
                if (!string.IsNullOrEmpty(command))
                {
                    List<Dictionary<string, object>> foundTriggers = GetTrigger(message.Sender, null, null, message.Sender.Nickname, command);
                    if (foundTriggers.Any())
                    {
                        foreach (Dictionary<string, object> foundTrigger in foundTriggers)
                        {
                            ExecuteCommand(MessageType.Query, message.Sender.Nickname, message.Sender, foundTrigger);
                        }
                    }
                }
            }
        }

        private void HandleChannelNotice(object sender, ChannelNotice message)
        {
            if (Enabled && !Bot.IsCommand(message.Message))
            {
                string command = Bot.GetCommand(message.Message);
                if (!string.IsNullOrEmpty(command))
                {
                    List<Dictionary<string, object>> foundTriggers = GetTrigger(message.Sender, null, message.Channel, null, command);
                    if (foundTriggers.Any())
                    {
                        foreach (Dictionary<string, object> foundTrigger in foundTriggers)
                        {
                            ExecuteCommand(MessageType.Notice, message.Channel, message.Sender, foundTrigger);
                        }
                    }
                }
            }
        }

        private void HandlePrivateNotice(object sender, PrivateNotice message)
        {
            if (Enabled && !Bot.IsCommand(message.Message))
            {
                string command = Bot.GetCommand(message.Message);
                if (!string.IsNullOrEmpty(command))
                {
                    List<Dictionary<string, object>> foundTriggers = GetTrigger(message.Sender, null, null, message.Sender.Nickname, command);
                    if (foundTriggers.Any())
                    {
                        foreach (Dictionary<string, object> foundTrigger in foundTriggers)
                        {
                            ExecuteCommand(MessageType.Notice, message.Sender.Nickname, message.Sender, foundTrigger);
                        }
                    }
                }
            }
        }

        /* Returns the parsed ID field if valid, otherwise returns 0 */
        private int HasValidCommandID(CommandMessage command)
        {
            int num = 0;
            int ret = 0;
            List<Dictionary<string, object>> foundTriggers = GetTrigger(command.Nick, "Self", string.Empty, string.Empty, null, true);

            if (int.TryParse(command.Arguments["ID"], out num))
            {
                if (foundTriggers.Count >= num && num > 0)
                {
                    ret = num;
                }
            }

            return ret;
        }

        private void AddCommand(CommandMessage command, string type, string permission, string channels, string nicknames, string trigger, string response)
        {
            if (!Bot.IsCommand(trigger))
            {
                int maxTriggers = Convert.ToInt32(GetOptionValue("Max Commands"));
                List<Dictionary<string, object>> currentCommands = GetTrigger(command.Nick, "Self", string.Empty, string.Empty, null, true);
                if (currentCommands.Count < maxTriggers)
                {
                    List<Dictionary<string, object>> foundTriggers = GetTrigger(command.Nick, permission, channels, nicknames, trigger, true);
                    if (!foundTriggers.Any())
                    {
                        AddNick(command.Nick);
                        string query = "INSERT INTO `customcommands` SET " +
                                       "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                                       "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {1} && `nicks`.`nickname` = {2}), " +
                                       "`type` = {3}, " +
                                       "`permission` = {4}, " +
                                       "`channels` = {5}, " +
                                       "`nicknames` = {6}, " +
                                       "`trigger` = {7}, " +
                                       "`response` = {8}, " +
                                       "`date_added` = {9}";
                        Bot.Database.Execute(query, new object[] {Bot.ServerConfig.Name, Bot.ServerConfig.Name, command.Nick.Nickname, type, permission, channels, nicknames, trigger, response, command.TimeStamp});
                        string message = string.Format("You now have \u0002{0}\u0002 custom commands set.", currentCommands.Count + 1);
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, message);
                    }
                    else
                    {
                        string errorMessage = string.Format("You already have a command set for \u0002{0}\u0002.", trigger);
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorMessage, true);
                    }
                }
                else
                {
                    string errorMessage = string.Format("You can not have more than \u0002{0}\u0002 custom commands set.  Please delete one before adding another.", maxTriggers);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorMessage, true);
                }
            }
            else
            {
                string errorMessage = string.Format("There is already a command for \u0002{0}\u0002.", trigger);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorMessage, true);
            }
        }

        private void DeleteCommand(CommandMessage command)
        {
            List<Dictionary<string, object>> foundTriggers = GetTrigger(command.Nick, "Self", string.Empty, string.Empty, null, true);
            int triggerIndex = HasValidCommandID(command);
            if (triggerIndex > 0)
            {
                string query = "DELETE FROM `customcommands` " +
                                "WHERE `id` = {0}";
                Bot.Database.Execute(query, new object[] { foundTriggers[triggerIndex - 1]["id"] });
                string message = string.Format("Command \u0002{0}\u0002 has been deleted.", triggerIndex);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, message);
            }
            else
            {
                string errorMessage = string.Format("\u0002{0}\u0002 is not a valid command number.", command.Arguments["ID"]);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorMessage, true);
            }
        }

        private void EditCommand(CommandMessage command, string type, string permission, string channels, string nicknames, string trigger, string response)
        {
            List<Dictionary<string, object>> foundTriggers = GetTrigger(command.Nick, "Self", string.Empty, string.Empty, null, true);
            int triggerIndex = HasValidCommandID(command);
            if (triggerIndex > 0)
            {
                string query = "UPDATE `customcommands` SET " +
                                "`type` = {0}, " +
                                "`permission` = {1}, " +
                                "`channels` = {2}, " +
                                "`nicknames` = {3}, " +
                                "`trigger` = {4}, " +
                                "`response` = {5} " +
                                "WHERE `id` = {6}";
                Bot.Database.Execute(query, new object[] { type, permission, channels, nicknames, trigger, response, foundTriggers[triggerIndex - 1]["id"] });
                string message = string.Format("\u0002{0}\u0002 now has the response: {1}", trigger, response);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, message);
            }
            else
            {
                string errorMessage = string.Format("\u0002{0}\u0002 is not a valid command number.", command.Arguments["ID"]);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorMessage, true);
            }
        }

        private void ViewTriggers(CommandMessage command)
        {
            List<Dictionary<string, object>> foundTriggers = GetTrigger(command.Nick, "Self", string.Empty, string.Empty, null, true);
            if (foundTriggers.Any())
            {
                int index = 1;
                foreach (Dictionary<string, object> foundTrigger in foundTriggers)
                {
                    string allowed = string.Empty;
                    switch (foundTrigger["permission"].ToString().ToLower())
                    {
                        case "channels":
                            allowed = " " + foundTrigger["channels"];
                            break;
                        case "nicks":
                            allowed = " " + foundTrigger["nicknames"];
                            break;
                    }
                    string response = string.Format("Command #{0} [{1}{2}] \u0002{3}{4}\u0002: {5}", index, foundTrigger["permission"], allowed, Bot.ServerConfig.CommandPrefix, foundTrigger["trigger"], foundTrigger["response"]);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, response, true);
                    index++;
                }
            }
            else
            {
                string errorMessage = "There are no custom commands for you.";
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorMessage, true);
            }
        }

        private void ViewTrigger(CommandMessage command, string trigger)
        {
            List<Dictionary<string, object>> foundTriggers = GetTrigger(command.Nick, null, string.Empty, string.Empty, trigger);
            if (foundTriggers.Any())
            {
                foreach (Dictionary<string, object> foundTrigger in foundTriggers)
                {
                    string allowed = string.Empty;
                    switch (foundTrigger["permission"].ToString().ToLower())
                    {
                        case "channels":
                            allowed = " " + foundTrigger["channels"];
                            break;
                        case "nicks":
                            allowed = " " + foundTrigger["nicknames"];
                            break;
                    }
                    string response = string.Format("[{0}{1}] \u0002{2}{3}\u0002: {4}", foundTrigger["permission"], allowed, Bot.ServerConfig.CommandPrefix, foundTrigger["trigger"], foundTrigger["response"]);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, response, true);
                }
            }
            else
            {
                string errorMessage = string.Format("\u0002{0}\u0002 is not set as a custom command for that search.", trigger);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorMessage, true);
            }
        }

        private List<Dictionary<string, object>> GetTrigger(Nick caller, string permission, string channels, string nicknames, string trigger = null, bool edit = false)
        {
            string search = "SELECT * FROM `customcommands`" +
                            " WHERE" +
                            " `server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0})";
            int argCount = 1;
            List<object> arguments = new List<object>();
            arguments.Add(Bot.ServerConfig.Name);
            if (!string.IsNullOrEmpty(trigger))
            {
                search += " AND `trigger` = {" + argCount++ + "}";
                arguments.Add(trigger);
            }
            search += " AND ";
            string combine = "AND";
            if (string.IsNullOrEmpty(permission))
            {
                search += "(";
                combine = "OR";
            }

            if (!string.IsNullOrEmpty(channels) && (string.IsNullOrEmpty(permission) || permission.ToLower() == "channels"))
            {
                Regex channelRegex = new Regex(@"(?<Prefix>[\#]+)?(?<Prefix>[\&]+)?(?<Channel>[^\#|^\&|^,]+)");
                MatchCollection matches = channelRegex.Matches(channels);
                if (matches.Count > 0)
                {
                    search += "(";
                    foreach (Match match in matches)
                    {
                        if (match.Success)
                        {
                            search += "`channels` REGEXP {" + argCount++ + "} OR ";
                            arguments.Add(string.Format(@"{0}[[:<:]]{1}[[:>:]]", string.Join(@"\\", match.Groups["Prefix"].ToString().ToCharArray()), match.Groups["Channel"]));
                        }
                    }
                    search += "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {" + argCount++ + "} && `nickname` = {" + argCount++ + "})";
                    search += (edit) ? ")" : " OR `permission` = 'all')";
                    arguments.Add(Bot.ServerConfig.Name);
                    arguments.Add(caller.Nickname);
                }
            }
            if (!string.IsNullOrEmpty(nicknames) && (string.IsNullOrEmpty(permission) || permission.ToLower() == "nicks"))
            {
                if (!string.IsNullOrEmpty(channels) && string.IsNullOrEmpty(permission))
                    search += " " + combine + " ";
                
                Regex nickRegex = new Regex(@"(?<Nickname>[^,]+)");
                MatchCollection matches = nickRegex.Matches(nicknames);
                if (matches.Count > 0)
                {
                    search += "(";
                    foreach (Match match in matches)
                    {
                        if (match.Success)
                        {
                            search += "`nicknames` REGEXP {" + argCount++ + "} OR ";
                            arguments.Add(string.Format(@"[[:<:]]{0}[[:>:]]", match.Groups["Nickname"]));
                        }
                    }
                    search += "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {" + argCount++ + "} && `nickname` = {" + argCount++ + "})";
                    search += (edit) ? ")" : " OR `permission` = 'all')";
                    arguments.Add(Bot.ServerConfig.Name);
                    arguments.Add(caller.Nickname);
                }
            }
            if (string.IsNullOrEmpty(permission) || permission.ToLower() == "self")
            {
                if (!string.IsNullOrEmpty(channels) || !string.IsNullOrEmpty(nicknames))
                    search += " " + combine + " ";
                search += "(`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {" + argCount++ + "} && `nickname` = {" + argCount++ + "})";
                search += (edit) ? ")" : " OR `permission` = 'all')";
                arguments.Add(Bot.ServerConfig.Name);
                arguments.Add(caller.Nickname);
            }
            if (string.IsNullOrEmpty(permission))
            {
                search += ")";
            }

            return Bot.Database.Query(search, arguments.ToArray());
        }

        private void ExecuteCommand(MessageType messageType, string location, Nick nick, Dictionary<string, object> trigger)
        {
            string type = trigger["type"].ToString();
            string message = trigger["response"].ToString();
            switch (type.ToLower())
            {
                case "response":
                    message = "\u200B" + message;
                    SendResponse(messageType, location, nick.Nickname, message);
                    break;
                case "command":
                    Bot.ExecuteCommand(message, location, messageType, nick);
                    break;
                case "list":
                    // todo handle list commands
                    break;
            }
        }
    }
}
