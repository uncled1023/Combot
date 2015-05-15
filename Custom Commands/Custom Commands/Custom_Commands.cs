using System;
using System.Collections.Generic;
using System.Linq;
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
                            string addTrigger = command.Arguments["Trigger"];
                            string addResponse = command.Arguments["Response"];
                            AddCommand(command, addTrigger, addResponse);
                            break;
                        case "del":
                            string delTrigger = command.Arguments["Trigger"];
                            DeleteCommand(command, delTrigger);
                            break;
                        case "edit":
                            string editTrigger = command.Arguments["Trigger"];
                            string editResponse = command.Arguments["Response"];
                            EditCommand(command, editTrigger, editResponse);
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
                    Dictionary<string, object> foundTrigger = GetTrigger(message.Sender, command);
                    if (foundTrigger != null)
                    {
                        ExecuteCommand(MessageType.Channel, message.Channel, message.Sender, foundTrigger);
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
                    Dictionary<string, object> foundTrigger = GetTrigger(message.Sender, command);
                    if (foundTrigger != null)
                    {
                        ExecuteCommand(MessageType.Query, message.Sender.Nickname, message.Sender, foundTrigger);
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
                    Dictionary<string, object> foundTrigger = GetTrigger(message.Sender, command);
                    if (foundTrigger != null)
                    {
                        ExecuteCommand(MessageType.Notice, message.Channel, message.Sender, foundTrigger);
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
                    Dictionary<string, object> foundTrigger = GetTrigger(message.Sender, command);
                    if (foundTrigger != null)
                    {
                        ExecuteCommand(MessageType.Notice, message.Sender.Nickname, message.Sender, foundTrigger);
                    }
                }
            }
        }

        private void AddCommand(CommandMessage command, string trigger, string response)
        {
            if (!Bot.IsCommand(trigger))
            {
                int maxTriggers = Convert.ToInt32(GetOptionValue("Max Commands"));
                List<Dictionary<string, object>> currentCommands = GetTriggers(command.Nick);
                if (currentCommands.Count < maxTriggers)
                {
                    Dictionary<string, object> foundTrigger = GetTrigger(command.Nick, trigger);
                    if (foundTrigger == null)
                    {
                        AddNick(command.Nick);
                        string query = "INSERT INTO `customcommands` SET " +
                                       "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                                       "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {1} && `nicks`.`nickname` = {2}), " +
                                       "`trigger` = {3}, " +
                                       "`response` = {4}, " +
                                       "`date_added` = {5}";
                        Bot.Database.Execute(query, new object[] {Bot.ServerConfig.Name, Bot.ServerConfig.Name, command.Nick.Nickname, trigger, response, command.TimeStamp});
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

        private void DeleteCommand(CommandMessage command, string trigger)
        {
            Dictionary<string, object> foundTrigger = GetTrigger(command.Nick, trigger);
            if (foundTrigger != null)
            {
                string query = "DELETE FROM `customcommands` " +
                               "WHERE `id` = {0}";
                Bot.Database.Execute(query, new object[] { foundTrigger["id"] });
                string message = string.Format("\u0002{0}\u0002 has been deleted.", trigger);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, message);
            }
            else
            {
                string errorMessage = string.Format("You do not have a command set for \u0002{0}\u0002.", trigger);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorMessage, true);
            }
        }

        private void EditCommand(CommandMessage command, string trigger, string response)
        {
            Dictionary<string, object> foundTrigger = GetTrigger(command.Nick, trigger);
            if (foundTrigger != null)
            {
                string query = "UPDATE `customcommands` SET " +
                                "`response` = {0} " +
                                "WHERE `id` = {1}";
                Bot.Database.Execute(query, new object[] { response, foundTrigger["id"] });
                string message = string.Format("\u0002{0}\u0002 now has the response: {1}", trigger, response);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, message);
            }
            else
            {
                string errorMessage = string.Format("You do not have a command set for \u0002{0}\u0002.", trigger);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorMessage, true);
            }
        }

        private void ViewTriggers(CommandMessage command)
        {
            List<Dictionary<string, object>> foundTriggers = GetTriggers(command.Nick);
            if (foundTriggers.Any())
            {
                int index = 1;
                foreach (Dictionary<string, object> foundTrigger in foundTriggers)
                {
                    string response = string.Format("Command #{0} \u0002{1}{2}\u0002: {3}", index, Bot.ServerConfig.CommandPrefix, foundTrigger["trigger"], foundTrigger["response"]);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, response, true);
                    index++;
                }
            }
            else
            {
                string errorMessage = "You do not have any custom commands.";
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorMessage, true);
            }
        }

        private void ViewTrigger(CommandMessage command, string trigger)
        {
            Dictionary<string, object> foundTrigger = GetTrigger(command.Nick, trigger);
            if (foundTrigger != null)
            {
                string response = string.Format("\u0002{0}{1}\u0002: {2}", Bot.ServerConfig.CommandPrefix, foundTrigger["trigger"], foundTrigger["response"]);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, response, true);
            }
            else
            {
                string errorMessage = string.Format("You do not have a command set for \u0002{0}\u0002.", trigger);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, errorMessage, true);
            }
        }

        private List<Dictionary<string, object>> GetTriggers(Nick caller)
        {
            string search = "SELECT * FROM `customcommands`" +
                           " WHERE" +
                           " `server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0})" +
                           " AND " +
                           " `nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {1} && `nickname` = {2})";
            return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, Bot.ServerConfig.Name, caller.Nickname });
        }

        private Dictionary<string, object> GetTrigger(Nick caller, string trigger)
        {
            string search = "SELECT * FROM `customcommands`" +
                           " WHERE" +
                           " `server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0})" +
                           " AND " +
                           " `nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {1} && `nickname` = {2})" +
                           " AND `trigger` = {3}";
            List<Dictionary<string, object>> results = Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, Bot.ServerConfig.Name, caller.Nickname, trigger });
            if (results.Any())
                return results.First();
            return null;
        }

        private void ExecuteCommand(MessageType messageType, string location, Nick nick, Dictionary<string, object> trigger)
        {
            string message = trigger["response"].ToString();
            if (message.StartsWith(Bot.ServerConfig.CommandPrefix))
            {
                Bot.ExecuteCommand(message, location, messageType, nick);
            }
            else
            {
                message = "\u200B" + message;
                SendResponse(messageType, location, nick.Nickname, message);
            }
        }
    }
}
