using System;
using System.Collections.Generic;
using System.Linq;
using Combot.Databases;
using Combot.IRCServices;
using Combot.IRCServices.Messaging;

namespace Combot.Modules.Plugins
{
    public class Messaging : Module
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
                case "Message":
                    AddMessage(command);
                    break;
                case "Anonymous Message":
                    AddMessage(command, true);
                    break;
            }
        }

        private void HandleChannelMessage(object sender, ChannelMessage message)
        {
            if (Enabled)
            {
                CheckMessages(message.Sender.Nickname);
            }
        }

        private void HandlePrivateMessage(object sender, PrivateMessage message)
        {
            if (Enabled)
            {
                CheckMessages(message.Sender.Nickname);
            }
        }

        private void HandleChannelNotice(object sender, ChannelNotice message)
        {
            if (Enabled)
            {
                CheckMessages(message.Sender.Nickname);
            }
        }

        private void HandlePrivateNotice(object sender, PrivateNotice message)
        {
            if (Enabled)
            {
                CheckMessages(message.Sender.Nickname);
            }
        }

        private void AddMessage(CommandMessage command, bool anonymous = false)
        {
            List<Dictionary<string, object>> currentMessages = GetSentMessages(command.Arguments["Nickname"], command.Nick.Nickname);
            int numMessages = currentMessages.Count();
            int maxMessages = Convert.ToInt32(GetOptionValue("Max Messages"));
            if (numMessages < maxMessages)
            {
                AddNick(command.Nick);
                Nick newNick = new Nick();
                Channel foundChannel = Bot.IRC.Channels.Find(chan => chan.Nicks.Exists(nick => nick.Nickname == command.Arguments["Nickname"]));
                if (foundChannel != null)
                {
                    newNick = foundChannel.GetNick(command.Arguments["Nickname"]);
                }
                else
                {
                    newNick.Nickname = command.Arguments["Nickname"];
                }
                AddNick(newNick);
                string query = "INSERT INTO `messages` SET " +
                               "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                               "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {1} && `nicks`.`nickname` = {2}), " +
                               "`sender_nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {3} && `nicks`.`nickname` = {4}), " +
                               "`message` = {5}, " +
                               "`anonymous` = {6}, " +
                               "`date_posted` = {7}";
                Bot.Database.Execute(query, new object[] { Bot.ServerConfig.Name, Bot.ServerConfig.Name, command.Arguments["Nickname"], Bot.ServerConfig.Name, command.Nick.Nickname, command.Arguments["Message"], anonymous, command.TimeStamp });
                string message = string.Format("I will send your message to \u0002{0}\u0002 as soon as I see them.", command.Arguments["Nickname"]);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, message);
            }
            else
            {
                string maxMessage = string.Format("You already have sent the maximum number of messages to \u0002{0}\u0002.  Wait until they have read their messages before sending another.", command.Arguments["Nickname"]);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, maxMessage, true);
            }
        }

        private void CheckMessages(string nickname)
        {
            List<Dictionary<string, object>> receivedMessages = GetReceivedMessages(nickname);
            if (receivedMessages.Any())
            {
                for (int i = 0; i < receivedMessages.Count; i++)
                {
                    DateTime dateSent = (DateTime)receivedMessages[i]["date_posted"];
                    string message = receivedMessages[i]["message"].ToString();
                    if ((bool) receivedMessages[i]["anonymous"])
                    {
                        Bot.IRC.Command.SendPrivateMessage(nickname, string.Format("An anonymous sender has left you a message on \u0002{0}\u0002", dateSent.ToString("MMMM d, yyyy h:mm:ss tt")));
                        Bot.IRC.Command.SendPrivateMessage(nickname, string.Format("\"{0}\"", message));
                    }
                    else
                    {
                        string sentNick = GetNickname((int) receivedMessages[i]["sender_nick_id"]);
                        Bot.IRC.Command.SendPrivateMessage(nickname, string.Format("\u0002{0}\u0002 has left you a message on \u0002{1}\u0002", sentNick, dateSent.ToString("MMMM d, yyyy h:mm:ss tt")));
                        Bot.IRC.Command.SendPrivateMessage(nickname, string.Format("\"{0}\"", message));
                        Bot.IRC.Command.SendPrivateMessage(nickname, string.Format("If you would like to reply to them, please type \u0002{0}{1} {2} \u001FMessage\u001F\u0002", Bot.ServerConfig.CommandPrefix, Commands.Find(cmd => cmd.Name == "Message").Triggers.First(), sentNick));
                    }
                    DeleteMessage((int) receivedMessages[i]["id"]);
                }
            }
        }

        private List<Dictionary<string, object>> GetSentMessages(string nick, string sender)
        {
            string search = "SELECT `messages`.`message`, `messages`.`nick_id`, `messages`.`date_posted`, `messages`.`anonymous` FROM `messages` WHERE " +
                            "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}) AND " +
                            "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {1} && `nickname` = {2}) AND " +
                            "`sender_nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {3} && `nickname` = {4})";
            return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, Bot.ServerConfig.Name, nick, Bot.ServerConfig.Name, sender });
        }

        private List<Dictionary<string, object>> GetReceivedMessages(string nick)
        {
            string search = "SELECT `messages`.`id`, `messages`.`message`, `messages`.`sender_nick_id`, `messages`.`date_posted`, `messages`.`anonymous` FROM `messages` " +
                            "INNER JOIN `nicks` " +
                            "ON `messages`.`nick_id` = `nicks`.`id` " +
                            "INNER JOIN `servers` " +
                            "ON `messages`.`server_id` = `servers`.`id` " +
                            "WHERE `servers`.`name` = {0} AND `nicks`.`nickname` = {1}";
            return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, nick });
        }

        private void DeleteMessage(int messageId)
        {
            string query = "DELETE FROM `messages` " +
                           "WHERE `id` = {0}";
            Bot.Database.Execute(query, new object[] { messageId });
        }
    }
}
