using System;
using System.Collections.Generic;
using System.Linq;
using Combot.Databases;

namespace Combot.Modules.Plugins
{
    public class Seen : Module
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
                case "Seen":
                    GetLastSeen(command);
                    break;
            }
        }

        private void GetLastSeen(CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            List<Dictionary<string, object>> channelList = GetChannelList(channel, command.Arguments["Nickname"]);
            List<Dictionary<string, object>> partList = GetPartList(channel, command.Arguments["Nickname"]);
            List<Dictionary<string, object>> joinList = GetJoinList(channel, command.Arguments["Nickname"]);
            List<Dictionary<string, object>> kickList = GetKickList(channel, command.Arguments["Nickname"]);
            List<Dictionary<string, object>> quitList = GetQuitList(command.Arguments["Nickname"]);

            List<Dictionary<DateTime, string>> lastSeenList = new List<Dictionary<DateTime, string>>();

            if (channelList.Any())
            {
                DateTime chanTime = (DateTime)channelList.First()["date_added"];
                TimeSpan difference = DateTime.Now.Subtract(chanTime);
                string message = string.Format("I last saw \u0002{0}\u0002 {1} ago saying the following in \u0002{2}\u0002: {3}", command.Arguments["Nickname"], ConvertToDifference(difference), channelList.First()["name"], channelList.First()["message"]);
                lastSeenList.Add(new Dictionary<DateTime, string>() { { chanTime, message } });
            }
            if (partList.Any())
            {
                DateTime partTime = (DateTime)partList.First()["date_added"];
                TimeSpan difference = DateTime.Now.Subtract(partTime);
                string message = string.Format("I last saw \u0002{0}\u0002 {1} ago leaving \u0002{2}\u0002.", command.Arguments["Nickname"], ConvertToDifference(difference), partList.First()["name"]);
                lastSeenList.Add(new Dictionary<DateTime, string>() { { partTime, message } });
            }
            if (joinList.Any())
            {
                DateTime joinTime = (DateTime)joinList.First()["date_added"];
                TimeSpan difference = DateTime.Now.Subtract(joinTime);
                string message = string.Format("I last saw \u0002{0}\u0002 {1} ago joining \u0002{2}\u0002.", command.Arguments["Nickname"], ConvertToDifference(difference), joinList.First()["name"]);
                lastSeenList.Add(new Dictionary<DateTime, string>() { { joinTime, message } });
            }
            if (kickList.Any())
            {
                DateTime kickTime = (DateTime)kickList.First()["date_added"];
                TimeSpan difference = DateTime.Now.Subtract(kickTime);
                string message = string.Format("I last saw \u0002{0}\u0002 {1} ago being kicked from \u0002{2}\u0002 with the reason: {3}", command.Arguments["Nickname"], ConvertToDifference(difference), kickList.First()["name"], kickList.First()["reason"]);
                lastSeenList.Add(new Dictionary<DateTime, string>() { { kickTime, message } });
            }
            if (quitList.Any())
            {
                DateTime quitTime = (DateTime)quitList.First()["date_added"];
                TimeSpan difference = DateTime.Now.Subtract(quitTime);
                string message = string.Format("I last saw \u0002{0}\u0002 {1} ago quiting.", command.Arguments["Nickname"], ConvertToDifference(difference));
                lastSeenList.Add(new Dictionary<DateTime, string>() { { quitTime, message } });
            }

            if (lastSeenList.Count > 0)
            {
                DateTime bestTime = new DateTime(1990);
                string seenMessage = string.Empty;
                for (int i = 0; i < lastSeenList.Count; i++)
                {
                    if (lastSeenList[i].Keys.First().CompareTo(bestTime) > 0)
                    {
                        bestTime = lastSeenList[i].Keys.First();
                        seenMessage = lastSeenList[i].Values.First();
                    }
                }
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

        private List<Dictionary<string, object>> GetChannelList(string channel, string nickname)
        {
            if (channel != null)
            {
                string search = "SELECT `channelmessages`.`message`, `channelmessages`.`date_added`, `channels`.`name` FROM `channelmessages` " +
                                "INNER JOIN `nicks` " +
                                "ON `channelmessages`.`nick_id` = `nicks`.`id` " +
                                "INNER JOIN `channels` " +
                                "ON `channelmessages`.`channel_id` = `channels`.`id` " +
                                "INNER JOIN `servers` " +
                                "ON `channelmessages`.`server_id` = `servers`.`id` " +
                                "WHERE `servers`.`name` = {0} AND `channels`.`name` = {1} AND `nicks`.`nickname` = {2} " +
                                "ORDER BY date_added DESC LIMIT 1";
                return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, channel, nickname });
            }
            else
            {
                string search = "SELECT `channelmessages`.`message`, `channelmessages`.`date_added`, `channels`.`name` FROM `channelmessages` " +
                                "INNER JOIN `nicks` " +
                                "ON `channelmessages`.`nick_id` = `nicks`.`id` " +
                                "INNER JOIN `channels` " +
                                "ON `channelmessages`.`channel_id` = `channels`.`id` " +
                                "INNER JOIN `servers` " +
                                "ON `channelmessages`.`server_id` = `servers`.`id` " +
                                "WHERE `servers`.`name` = {0} AND `nicks`.`nickname` = {1} " +
                                "ORDER BY date_added DESC LIMIT 1";
                return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, nickname });   
            }
        }

        private List<Dictionary<string, object>> GetPartList(string channel, string nickname)
        {
            if (channel != null)
            {
                string search = "SELECT `channelparts`.`date_added`, `channels`.`name` FROM `channelparts` " +
                                "INNER JOIN `nicks` " +
                                "ON `channelparts`.`nick_id` = `nicks`.`id` " +
                                "INNER JOIN `channels` " +
                                "ON `channelparts`.`channel_id` = `channels`.`id` " +
                                "INNER JOIN `servers` " +
                                "ON `channelparts`.`server_id` = `servers`.`id` " +
                                "WHERE `servers`.`name` = {0} AND `channels`.`name` = {1} AND `nicks`.`nickname` = {2} " +
                                "ORDER BY date_added DESC LIMIT 1";
                return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, channel, nickname });
            }
            else
            {
                string search = "SELECT `channelparts`.`date_added`, `channels`.`name` FROM `channelparts` " +
                                "INNER JOIN `nicks` " +
                                "ON `channelparts`.`nick_id` = `nicks`.`id` " +
                                "INNER JOIN `channels` " +
                                "ON `channelparts`.`channel_id` = `channels`.`id` " +
                                "INNER JOIN `servers` " +
                                "ON `channelparts`.`server_id` = `servers`.`id` " +
                                "WHERE `servers`.`name` = {0} AND `nicks`.`nickname` = {1} " +
                                "ORDER BY date_added DESC LIMIT 1";
                return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, nickname });
            }
        }

        private List<Dictionary<string, object>> GetJoinList(string channel, string nickname)
        {
            if (channel != null)
            {
                string search = "SELECT `channeljoins`.`date_added`, `channels`.`name` FROM `channeljoins` " +
                                "INNER JOIN `nicks` " +
                                "ON `channeljoins`.`nick_id` = `nicks`.`id` " +
                                "INNER JOIN `channels` " +
                                "ON `channeljoins`.`channel_id` = `channels`.`id` " +
                                "INNER JOIN `servers` " +
                                "ON `channeljoins`.`server_id` = `servers`.`id` " +
                                "WHERE `servers`.`name` = {0} AND `channels`.`name` = {1} AND `nicks`.`nickname` = {2} " +
                                "ORDER BY date_added DESC LIMIT 1";
                return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, channel, nickname });
            }
            else
            {
                string search = "SELECT `channeljoins`.`date_added`, `channels`.`name` FROM `channeljoins` " +
                                "INNER JOIN `nicks` " +
                                "ON `channeljoins`.`nick_id` = `nicks`.`id` " +
                                "INNER JOIN `channels` " +
                                "ON `channeljoins`.`channel_id` = `channels`.`id` " +
                                "INNER JOIN `servers` " +
                                "ON `channeljoins`.`server_id` = `servers`.`id` " +
                                "WHERE `servers`.`name` = {0} AND `nicks`.`nickname` = {1} " +
                                "ORDER BY date_added DESC LIMIT 1";
                return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, nickname });
            }
        }

        private List<Dictionary<string, object>> GetKickList(string channel, string nickname)
        {
            if (channel != null)
            {
                string search = "SELECT `channelkicks`.`date_added`, `channelkicks`.`reason`, `channels`.`name` FROM `channelkicks` " +
                                "INNER JOIN `nicks` " +
                                "ON `channelkicks`.`kicked_nick_id` = `nicks`.`id` " +
                                "INNER JOIN `channels` " +
                                "ON `channelkicks`.`channel_id` = `channels`.`id` " +
                                "INNER JOIN `servers` " +
                                "ON `channelkicks`.`server_id` = `servers`.`id` " +
                                "WHERE `servers`.`name` = {0} AND `channels`.`name` = {1} AND `nicks`.`nickname` = {2} " +
                                "ORDER BY date_added DESC LIMIT 1";
                return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, channel, nickname });
            }
            else
            {
                string search = "SELECT `channelkicks`.`date_added`, `channelkicks`.`reason`, `channels`.`name` FROM `channelkicks` " +
                                "INNER JOIN `nicks` " +
                                "ON `channelkicks`.`kicked_nick_id` = `nicks`.`id` " +
                                "INNER JOIN `channels` " +
                                "ON `channelkicks`.`channel_id` = `channels`.`id` " +
                                "INNER JOIN `servers` " +
                                "ON `channelkicks`.`server_id` = `servers`.`id` " +
                                "WHERE `servers`.`name` = {0} AND `nicks`.`nickname` = {1} " +
                                "ORDER BY date_added DESC LIMIT 1";
                return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, nickname });
            }
        }

        private List<Dictionary<string, object>> GetQuitList(string nickname)
        {
            string search = "SELECT `quits`.`date_added` FROM `quits` " +
                            "INNER JOIN `nicks` " +
                            "ON `quits`.`nick_id` = `nicks`.`id` " +
                            "INNER JOIN `servers` " +
                            "ON `quits`.`server_id` = `servers`.`id` " +
                            "WHERE `servers`.`name` = {0} AND `nicks`.`nickname` = {1} " +
                            "ORDER BY date_added DESC LIMIT 1";
            return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, nickname });
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
