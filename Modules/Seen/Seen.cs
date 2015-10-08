using System;
using System.Collections.Generic;
using System.Linq;
using Combot.Databases;
using System.IO;
using Combot.IRCServices.Messaging;
using Combot.IRCServices;

namespace Combot.Modules.Plugins
{
    public class Seen : Module
    {
        public override void Initialize()
        {
            InitializeTable();
            Bot.CommandReceivedEvent += HandleCommandEvent;

            // Handle nick events and update last seen
            Bot.IRC.Message.CTCPMessageReceivedEvent += CTCPRelayHandlerHandler;
            Bot.IRC.Message.CTCPNoticeReceivedEvent += CTCPRelayHandlerHandler;
            Bot.IRC.Message.ChannelMessageReceivedEvent += ChannelMessageHandler;
            Bot.IRC.Message.ChannelNoticeReceivedEvent += ChannelNoticeHandler;
            Bot.IRC.Message.JoinChannelEvent += ChannelJoinHandler;
            Bot.IRC.Message.InviteChannelEvent += ChannelInviteHandler;
            Bot.IRC.Message.PartChannelEvent += ChannelPartHandler;
            Bot.IRC.Message.KickEvent += ChannelKickHandler;
            Bot.IRC.Message.TopicChangeEvent += TopicChangeHandler;
            Bot.IRC.Message.QuitEvent += QuitHandler;
            Bot.IRC.Message.NickChangeEvent += NickChangeHandler;
        }

        public override void ParseCommand(CommandMessage command)
        {
            Command foundCommand = Commands.Find(c => c.Triggers.Contains(command.Command));
            switch (foundCommand.Name)
            {
                case "Seen":
                    GetLastSeen(command);
                    break;
            }
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

        private void GetLastSeen(CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;

            List<Dictionary<string, object>> lastSeenList = GetSeenList(channel, command.Arguments["Nickname"]);

            if (lastSeenList.Any())
            {
                Dictionary<string, object> lastSeen = lastSeenList.First();
                DateTime bestTime = DateTime.Now;
                DateTime.TryParse(lastSeen["date_seen"].ToString(), out bestTime);
                string seenMessage = string.Format("I last saw \u0002{0}\u0002 {1} ago {2}", command.Arguments["Nickname"], ConvertToDifference(DateTime.Now.Subtract(bestTime)), lastSeen["message"].ToString());
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, seenMessage);
            }
            else
            {
                string notFound = string.Empty;
                if (channel != null)
                {
                    notFound = string.Format("I have not seen \u0002{0}\u0002 in \u0002{1}\u0002.", command.Arguments["Nickname"], channel);
                }
                else
                {
                    notFound = string.Format("I have not seen \u0002{0}\u0002.", command.Arguments["Nickname"]);
                }
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, notFound);
            }
        }

        private List<Dictionary<string, object>> GetSeenList(string channel, string nickname)
        {

            string search = "SELECT `seen`.`id`, `seen`.`date_seen`, `seen`.`message`, `channels`.`name` FROM `seen` " +
                            "INNER JOIN `nicks` " +
                            "ON `seen`.`nick_id` = `nicks`.`id` " +
                            "INNER JOIN `channels` " +
                            "ON `seen`.`channel_id` = `channels`.`id` " +
                            "INNER JOIN `servers` " +
                            "ON `seen`.`server_id` = `servers`.`id` ";
            if (channel != null)
            {
                search += "WHERE `servers`.`name` = {0} AND `channels`.`name` = {1} AND `nicks`.`nickname` = {2} " +
                          "ORDER BY date_seen DESC";
                return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, channel, nickname });
            }
            else
            {
                search += "WHERE `servers`.`name` = {0} AND `nicks`.`nickname` = {1} " +
                          "ORDER BY date_seen DESC";
                return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, nickname });
            }
        }

        private void NickChangeHandler(object sender, NickChangeInfo e)
        {
            string message = string.Format("changing nicks to \u0002{0}\u0002", e.NewNick);
            UpdateSeen(null, e.OldNick, message, e.TimeStamp);
        }

        private void QuitHandler(object sender, QuitInfo e)
        {
            string message = string.Format("quitting ({0})", e.Message);
            UpdateSeen(null, e.Nick, message, e.TimeStamp);
        }

        private void TopicChangeHandler(object sender, TopicChangeInfo e)
        {
            string message = string.Format("changing the topic in \u0002{0}\u0002 to: {1}", e.Channel, e.Topic);
            UpdateSeen(e.Channel, e.Nick, message, e.TimeStamp);
        }

        private void ChannelKickHandler(object sender, KickInfo e)
        {
            string message = string.Format("being kicked from \u0002{0}\u0002 by \u0002{1}\u0002 for the reason: {2}", e.Channel, e.Nick.Nickname, e.Reason);
            UpdateSeen(e.Channel, e.KickedNick, message, e.TimeStamp);
        }

        private void ChannelPartHandler(object sender, PartChannelInfo e)
        {
            string message = string.Format("parting \u0002{0}\u0002", e.Channel);
            UpdateSeen(e.Channel, e.Nick, message, e.TimeStamp);
        }

        private void ChannelInviteHandler(object sender, InviteChannelInfo e)
        {
            string message = string.Format("inviting \u0002{0}\u0002 into \u0002{0}\u0002", e.Recipient.Nickname, e.Channel);
            UpdateSeen(e.Channel, e.Requester, message, e.TimeStamp);
        }

        private void ChannelJoinHandler(object sender, JoinChannelInfo e)
        {
            string message = string.Format("joining \u0002{0}\u0002", e.Channel);
            UpdateSeen(e.Channel, e.Nick, message, e.TimeStamp);
        }

        private void ChannelNoticeHandler(object sender, ChannelNotice e)
        {
            string message = string.Format("saying the following notice in \u0002{0}\u0002: {1}", e.Channel, e.Message);
            UpdateSeen(e.Channel, e.Sender, message, e.TimeStamp);
        }

        private void ChannelMessageHandler(object sender, ChannelMessage e)
        {
            string message = string.Format("saying the following in \u0002{0}\u0002: {1}", e.Channel, e.Message);
            UpdateSeen(e.Channel, e.Sender, message, e.TimeStamp);
        }

        private void CTCPRelayHandlerHandler(object sender, CTCPMessage e)
        {
            string message = string.Format("saying the following CTCP command in \u0002{0}\u0002: [{1}] {2}", e.Location, e.Command, e.Arguments);
            UpdateSeen(e.Location, e.Sender, message, e.TimeStamp);
        }

        private void UpdateSeen(string channel, Nick nick, string message, DateTime time)
        {
            List<Dictionary<string, object>> results = GetSeenList(channel, nick.Nickname);
            if (results.Any())
            {
                foreach (Dictionary<string, object> row in results)
                {
                    // Update the table
                    string query = "UPDATE `seen` SET " +
                                   "`message` = {0}, " +
                                   "`date_seen` = {1} " +
                                   "WHERE `id` = {2}";
                    Bot.Database.Execute(query, new object[] { message, time, row["id"].ToString() });
                }
            }
            else
            {
                // Add a new record
                AddNick(nick);
                AddChannel(channel);
                string query = "INSERT INTO `seen` SET " +
                               "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                               "`channel_id` = (SELECT `channels`.`id` FROM `channels` INNER JOIN `servers` ON `servers`.`id` = `channels`.`server_id` WHERE `servers`.`name` = {1} && `channels`.`name` = {2}), " +
                               "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {3} && `nickname` = {4}), " +
                               "`message` = {5}, " +
                               "`date_seen` = {6}";
                Bot.Database.Execute(query, new object[] { Bot.ServerConfig.Name, Bot.ServerConfig.Name, channel, Bot.ServerConfig.Name, nick.Nickname, message, time });
            }
        }

        private string ConvertToDifference(TimeSpan time)
        {
            string timeString = string.Empty;
            if (time.Days != 0)
            {
                string plural = string.Empty;
                if (time.Days > 1)
                {
                    plural = "s";
                }
                timeString += string.Format("{0} Day{1} ", time.Days, plural);
            }
            if (time.Hours != 0)
            {
                string plural = string.Empty;
                if (time.Hours > 1)
                {
                    plural = "s";
                }
                timeString += string.Format("{0} Hour{1} ", time.Hours, plural);
            }
            if (time.Minutes != 0)
            {
                string plural = string.Empty;
                if (time.Minutes > 1)
                {
                    plural = "s";
                }
                timeString += string.Format("{0} Minute{1} ", time.Minutes, plural);
            }
            if (time.Seconds != 0)
            {
                string plural = string.Empty;
                if (time.Seconds > 1)
                {
                    plural = "s";
                }
                timeString += string.Format("{0} Second{1} ", time.Seconds, plural);
            }

            return timeString.Trim();
        }
    }
}
