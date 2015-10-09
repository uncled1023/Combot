using Combot.IRCServices;
using System;
using System.Collections.Generic;
using System.Linq;
using Combot.IRCServices.Messaging;
using Combot.IRCServices.Commanding;
using System.IO;

namespace Combot.Modules.Plugins
{
    public class Relay : Module
    {
        public override void Initialize()
        {
            InitializeTable();

            Bot.CommandReceivedEvent += HandleCommandEvent;

            // Incoming Messages
            Bot.IRC.Message.CTCPMessageReceivedEvent += CTCPRelayHandler;
            Bot.IRC.Message.CTCPNoticeReceivedEvent += CTCPRelayHandler;
            Bot.IRC.Message.ChannelMessageReceivedEvent += RelayChannelMessage;
            Bot.IRC.Message.PrivateMessageReceivedEvent += RelayPrivateMessage;
            Bot.IRC.Message.ChannelNoticeReceivedEvent += RelayChannelNotice;
            Bot.IRC.Message.PrivateNoticeReceivedEvent += RelayPrivateNotice;
            Bot.IRC.Message.ChannelModeChangeEvent += RelayChannelMode;
            Bot.IRC.Message.UserModeChangeEvent += RelayUserMode;
            Bot.IRC.Message.JoinChannelEvent += RelayChannelJoin;
            Bot.IRC.Message.InviteChannelEvent += RelayChannelInvite;
            Bot.IRC.Message.PartChannelEvent += RelayChannelPart;
            Bot.IRC.Message.KickEvent += RelayChannelKick;
            Bot.IRC.Message.TopicChangeEvent += RelayTopicChange;
            Bot.IRC.Message.QuitEvent += RelayQuit;
            Bot.IRC.Message.NickChangeEvent += RelayNickChange;

            // Outgoing messages
            //Bot.IRC.Command.CTCPMessageCommandEvent += RelayCTCPMessageCommand;
            //Bot.IRC.Command.CTCPNoticeCommandEvent += RelayCTCPNoticeCommand;
            //Bot.IRC.Command.PrivateMessageCommandEvent += RelayPrivateMessageCommand;
            //Bot.IRC.Command.PrivateNoticeCommandEvent += RelayPrivateNoticeCommand;
            //Bot.IRC.Command.ChannelModeCommandEvent += RelayChannelModeCommand;
            //Bot.IRC.Command.UserModeCommandEvent += RelayUserModeCommand;
            //Bot.IRC.Command.KickCommandEvent += RelayKickCommand;
            //Bot.IRC.Command.InviteCommandEvent += RelayInviteCommand;
            //Bot.IRC.Command.PartCommandEvent += RelayPartCommand;
            //Bot.IRC.Command.TopicCommandEvent += RelayTopicCommand;
            //Bot.IRC.Command.JoinCommandEvent += RelayJoinCommand;
            //Bot.IRC.Command.NickCommandEvent += RelayNickCommand;
        }

        private void InitializeTable()
        {
            string sqlPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigPath, "CreateTable.sql");
            if (File.Exists(sqlPath))
            {
                string query = File.ReadAllText(sqlPath);
                Bot.Database.Execute(query);
            }
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
            string chanAccess = GetOptionValue("Channel Access").ToString();
            AccessType access = AccessType.User;
            Enum.TryParse(chanAccess, out access);

            // verify access in source and target
            if (!CheckAccess(source, command.Nick.Nickname, access))
            {
                string invalid = string.Format("You do not have permission to use '{0}' as a source.", source);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid, true);
                return;
            }
            if (Channel.IsChannel(target) && !CheckAccess(target, command.Nick.Nickname, access))
            {
                string invalid = string.Format("You do not have permission to use '{0}' as a target.", source);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid, true);
                return;
            }

            RelayType relayType = RelayType.Message;
            Enum.TryParse(type, true, out relayType);

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
            string chanAccess = GetOptionValue("Channel Access").ToString();
            AccessType access = AccessType.User;
            Enum.TryParse(chanAccess, out access);

            // verify access in source and target
            if (!CheckAccess(source, command.Nick.Nickname, access))
            {
                string invalid = string.Format("You do not have permission to use '{0}' as a source.", source);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid, true);
                return;
            }
            if (Channel.IsChannel(target) && !CheckAccess(target, command.Nick.Nickname, access))
            {
                string invalid = string.Format("You do not have permission to use '{0}' as a target.", source);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid, true);
                return;
            }

            RelayType relayType = RelayType.Message;
            Enum.TryParse(type, true, out relayType);

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
            string search = "SELECT `relays`.`id`, `relays`.`source`, `relays`.`target`, `relays`.`type`, `relays`.`modes` FROM `relays` " +
                            "INNER JOIN `nicks` " +
                            "ON `relays`.`nick_id` = `nicks`.`id` " +
                            "INNER JOIN `servers` " +
                            "ON `nicks`.`server_id` = `servers`.`id` " +
                            "WHERE `servers`.`name` = {0} AND `nicks`.`nickname` = {1}";
            return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, nickname });
        }

        private List<Dictionary<string, object>> GetRelayList(string source, RelayType type)
        {
            string search = "SELECT `relays`.`target`, `relays`.`modes` FROM `relays` " +
                            "INNER JOIN `servers` " +
                            "ON `relays`.`server_id` = `servers`.`id` " +
                            "WHERE `servers`.`name` = {0} AND `relays`.`source` = {1} AND `relays`.`type` = {2}";
            return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, source, (int)type });
        }

        private bool CheckAccess(string source, string nick, AccessType access)
        {
            // Owners get to have all the fun
            if (Bot.ServerConfig.Owners.Contains(nick))
                return true;

            // The source is a channel
            if (Bot.IRC.Channels.Exists(chan => chan.Name == source))
            {
                bool valid = Bot.CheckChannelAccess(source, nick, AccessType.Operator);
                if (!valid)
                    return false;
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

        private void ProcessRelay(string source, RelayType type, string message, List<UserModeInfo> userModes = null, List<ChannelModeInfo> channelModes = null)
        {
            List<Dictionary<string, object>> relays = GetRelayList(source, type);
            switch (type)
            {
                case RelayType.Mode:
                    for (int i = 0; i < relays.Count; i++)
                    {
                        string modeStr = relays[i]["modes"].ToString();
                        char[] modes = modeStr.ToCharArray();
                        bool modeFound = false;
                        foreach (char mode in modes)
                        {
                            if (userModes != null)
                            {
                                if (userModes.Exists(info => info.Mode.ToString() == mode.ToString()))
                                {
                                    modeFound = true;
                                    break;
                                }
                            }
                            if (channelModes != null)
                            {
                                if (channelModes.Exists(info => info.Mode.ToString() == mode.ToString()))
                                {
                                    modeFound = true;
                                    break;
                                }
                            }
                        }
                        if (!modeFound)
                        {
                            relays.RemoveAt(i);
                            i--;
                        }
                    }
                    break;
                default:
                    break;
            }
            foreach (Dictionary<string, object> relay in relays)
            {
                string target = relay["target"].ToString();
                MessageType msgType = MessageType.Channel;
                if (!Channel.IsChannel(target))
                    msgType = MessageType.Query;
                SendResponse(msgType, target, target, message);
            }
        }

        #region Incomming Messages
        private void RelayQuit(object sender, QuitInfo e)
        {
            string msg = string.Format(" * {0} has quit. ({1})", e.Nick.Nickname, e.Message);
            ProcessRelay(e.Nick.Nickname, RelayType.Quit, msg);
        }

        private void RelayTopicChange(object sender, TopicChangeInfo e)
        {
            string msg = string.Format("[{0}] {1} has changed the topic to: {2}.", e.Channel, e.Nick.Nickname, e.Topic);
            ProcessRelay(e.Channel, RelayType.Topic, msg);
        }

        private void RelayChannelKick(object sender, KickInfo e)
        {
            string msg = string.Format("[{0}] * {1} has kicked {2} ({3})", e.Channel, e.Nick.Nickname, e.KickedNick.Nickname, e.Reason);
            ProcessRelay(e.Channel, RelayType.Kick, msg);
        }

        private void RelayChannelPart(object sender, PartChannelInfo e)
        {
            string msg = string.Format("[{0}] * {1} has left.", e.Channel, e.Nick.Nickname);
            ProcessRelay(e.Channel, RelayType.Part, msg);
        }

        private void RelayChannelInvite(object sender, InviteChannelInfo e)
        {
            string msg = string.Format("[{0}] * {1} invited {2}", e.Channel, e.Requester.Nickname, e.Recipient.Nickname);
            ProcessRelay(e.Channel, RelayType.Invite, msg);
        }

        private void RelayChannelJoin(object sender, JoinChannelInfo e)
        {
            string msg = string.Format("[{0}] * {1} ({2}) has joined.", e.Channel, e.Nick.Nickname, e.Nick.Host);
            ProcessRelay(e.Channel, RelayType.Join, msg);
        }

        private void RelayUserMode(object sender, UserModeChangeInfo e)
        {
            string msg = string.Format(" * {0} sets mode {1}", e.Nick.Nickname, e.Modes.ModesToString());
            ProcessRelay(e.Nick.Nickname, RelayType.Mode, msg, e.Modes);
        }

        private void RelayChannelMode(object sender, ChannelModeChangeInfo e)
        {
            string msg = string.Format("[{0}] * {1} sets mode {2} on {3}.", e.Channel, e.Nick.Nickname, e.Modes.ModesToString(), e.Channel);
            ProcessRelay(e.Channel, RelayType.Mode, msg, null, e.Modes);
        }

        private void RelayPrivateNotice(object sender, PrivateNotice e)
        {
            string msg = string.Format("[-{0}-] {1}", e.Sender.Nickname, e.Message);
            ProcessRelay(e.Sender.Nickname, RelayType.Message, msg);
        }

        private void RelayChannelNotice(object sender, ChannelNotice e)
        {
            string msg = string.Format("[{0}] [-{1}-] {2}", e.Channel, e.Sender.Nickname, e.Message);
            ProcessRelay(e.Channel, RelayType.Message, msg);
        }

        private void RelayPrivateMessage(object sender, PrivateMessage e)
        {
            string msg = string.Format("[{0}] {1}", e.Sender.Nickname, e.Message);
            ProcessRelay(e.Sender.Nickname, RelayType.Message, msg);
        }

        private void RelayChannelMessage(object sender, ChannelMessage e)
        {
            string msg = string.Format("[{0}] [{1}] {2}", e.Channel, e.Sender.Nickname, e.Message);
            ProcessRelay(e.Channel, RelayType.Message, msg);
        }

        private void CTCPRelayHandler(object sender, CTCPMessage e)
        {
            string msg = string.Format("[{0}] [CTCP] <{1}> {2}", e.Sender.Nickname, e.Command, e.Arguments);
            ProcessRelay(e.Location, RelayType.CTCP, msg);
        }

        private void RelayNickChange(object sender, NickChangeInfo e)
        {
            string msg = string.Format(" * {0} is now known as {1}", e.OldNick.Nickname, e.NewNick.Nickname);
            ProcessRelay(e.NewNick.Nickname, RelayType.Nick, msg);
        }
        #endregion

        #region Outgoing Commands
        private void RelayPrivateNoticeCommand(object sender, PrivateNoticeCommand e)
        {
            string msg = string.Format("[-{0}-] {1}", Bot.IRC.Nickname, e.Message);
            ProcessRelay(e.Recipient, RelayType.Message, msg);
        }

        private void RelayPrivateMessageCommand(object sender, PrivateMessageCommand e)
        {
            string msg = string.Format("[{0}] {1}", Bot.IRC.Nickname, e.Message);
            //ProcessRelay(e.Recipient, RelayType.Message, msg);
        }

        private void RelayUserModeCommand(object sender, UserModeCommand e)
        {
            string msg = string.Format(" * {0} sets mode {1}", e.Nick, e.Mode.ModeToString());
            ProcessRelay(e.Nick, RelayType.Mode, msg, new List<UserModeInfo> { e.Mode });
        }

        private void RelayChannelModeCommand(object sender, ChannelModeCommand e)
        {
            string msg = string.Format("[{0}] * {1} sets mode {2} on {3}.", e.Channel, Bot.IRC.Nickname, e.Mode.ModeToString(), e.Channel);
            ProcessRelay(e.Channel, RelayType.Mode, msg, null, new List<ChannelModeInfo> { e.Mode });
        }

        private void RelayCTCPNoticeCommand(object sender, CTCPNoticeCommand e)
        {
            string msg = string.Format("[-{0}-] [CTCP] <{1}> {2}", Bot.IRC.Nickname, e.Command, e.Arguments);
            ProcessRelay(e.Recipient, RelayType.CTCP, msg);
        }

        private void RelayCTCPMessageCommand(object sender, CTCPMessageCommand e)
        {
            string msg = string.Format("[{0}] [CTCP] <{1}> {2}", Bot.IRC.Nickname, e.Command, e.Arguments);
            ProcessRelay(e.Recipient, RelayType.CTCP, msg);
        }

        private void RelayNickCommand(object sender, NickCommand e)
        {
            string msg = string.Format(" * {0} is now known as {1}", Bot.IRC.Nickname, e.Nick);
            ProcessRelay(Bot.IRC.Nickname, RelayType.Nick, msg);
        }

        private void RelayJoinCommand(object sender, JoinCommand e)
        {
            string msg = string.Format("[{0}] * {1} has joined.", e.Channel, Bot.IRC.Nickname);
            ProcessRelay(e.Channel, RelayType.Join, msg);
        }

        private void RelayTopicCommand(object sender, TopicCommand e)
        {
            string msg = string.Format("[{0}] {1} has changed the topic to: {2}.", e.Channel, Bot.IRC.Nickname, e.Topic);
            ProcessRelay(e.Channel, RelayType.Topic, msg);
        }

        private void RelayPartCommand(object sender, PartCommand e)
        {
            string msg = string.Format("[{0}] * {1} has left.", e.Channel, Bot.IRC.Nickname);
            ProcessRelay(e.Channel, RelayType.Part, msg);
        }

        private void RelayKickCommand(object sender, KickCommand e)
        {
            string msg = string.Format("[{0}] * {1} has kicked {2} ({3})", e.Channel, Bot.IRC.Nickname, e.Nick, e.Reason);
            ProcessRelay(e.Channel, RelayType.Kick, msg);
        }

        private void RelayInviteCommand(object sender, InviteCommand e)
        {
            string msg = string.Format("[{0}] * {1} invited {2}", e.Channel, Bot.IRC.Nickname, e.Nick);
            ProcessRelay(e.Channel, RelayType.Invite, msg);
        }
        #endregion
    }
}