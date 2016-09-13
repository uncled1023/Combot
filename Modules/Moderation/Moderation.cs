using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Combot.IRCServices;
using Timer = System.Timers.Timer;
using System.IO;
using Combot.IRCServices.Messaging;

namespace Combot.Modules.Plugins
{
    public class Moderation : Module
    {
        private List<Timer> unbanTimers;
        private ReaderWriterLockSlim listLock;

        public override void Initialize()
        {
            string sqlPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigPath, "CreateTable.sql");
            if (File.Exists(sqlPath))
            {
                string query = File.ReadAllText(sqlPath);
                Bot.Database.Execute(query);
            }

            unbanTimers = new List<Timer>();
            listLock = new ReaderWriterLockSlim();
            Bot.CommandReceivedEvent += HandleCommandEvent;
            Bot.IRC.Message.JoinChannelEvent += HandleJoinEvent;
            Bot.IRC.Message.ChannelMessageReceivedEvent += HandleChannelMessageEvent;
            Bot.IRC.Message.ChannelNoticeReceivedEvent += HandleChannelNoticeEvent;
        }

        public override void ParseCommand(CommandMessage command)
        {
            Command foundCommand = Commands.Find(c => c.Triggers.Contains(command.Command));

            switch (foundCommand.Name)
            {
                // Privilege Mode Commands
                case "Founder":
                    ModifyUserPrivilege(true, command, ChannelMode.q);
                    break;
                case "Remove Founder":
                    ModifyUserPrivilege(false, command, ChannelMode.q);
                    break;
                case "SOP":
                    ModifyUserPrivilege(true, command, ChannelMode.a);
                    break;
                case "Remove SOP":
                    ModifyUserPrivilege(false, command, ChannelMode.a);
                    break;
                case "OP":
                    ModifyUserPrivilege(true, command, ChannelMode.o);
                    break;
                case "Remove OP":
                    ModifyUserPrivilege(false, command, ChannelMode.o);
                    break;
                case "HOP":
                    ModifyUserPrivilege(true, command, ChannelMode.h);
                    break;
                case "Remove HOP":
                    ModifyUserPrivilege(false, command, ChannelMode.h);
                    break;
                case "Voice":
                    ModifyUserPrivilege(true, command, ChannelMode.v);
                    break;
                case "Remove Voice":
                    ModifyUserPrivilege(false, command, ChannelMode.v);
                    break;
                // Auto Privilege Management
                case "ASOP":
                    ModifyAutoUserPrivilege("SOP", command, ChannelMode.a);
                    break;
                case "AOP":
                    ModifyAutoUserPrivilege("AOP", command, ChannelMode.o);
                    break;
                case "AHOP":
                    ModifyAutoUserPrivilege("HOP", command, ChannelMode.h);
                    break;
                case "AVoice":
                    ModifyAutoUserPrivilege("VOP", command, ChannelMode.v);
                    break;
                // Channel Moderation
                case "Mode":
                    ModifyChannelModes(foundCommand, command);
                    break;
                case "Topic":
                    ModifyChannelTopic(foundCommand, command);
                    break;
                case "Invite":
                    InviteNick(foundCommand, command);
                    break;
                case "Auto Ban":
                    AutoBan(foundCommand, command);
                    break;
                case "Ban":
                    BanNick(true, foundCommand, command);
                    break;
                case "UnBan":
                    BanNick(false, foundCommand, command);
                    break;
                case "Kick Ban":
                    BanNick(true, foundCommand, command);
                    KickNick(foundCommand, command);
                    break;
                case "Timed Ban":
                    TimedBan(foundCommand, command);
                    break;
                case "Timed Kick Ban":
                    TimedBan(foundCommand, command);
                    KickNick(foundCommand, command);
                    break;
                case "Kick":
                    KickNick(foundCommand, command);
                    break;
                case "Kick Self":
                    KickSelf(command);
                    break;
                case "Clear":
                    ClearChannel(foundCommand, command);
                    break;
                case "Roll Call":
                    string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
                    Channel foundChannel = Bot.IRC.Channels.Find(chan => chan.Name == channel);
                    if (foundChannel != null)
                    {
                        string rollCall = string.Join(", ", foundChannel.Nicks.Select(nick => nick.Nickname));
                        Bot.IRC.Command.SendPrivateMessage(channel, "It's time for a Roll Call!");
                        Bot.IRC.Command.SendPrivateMessage(channel, rollCall);
                    }
                    else
                    {
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, string.Format("I am not in \u0002{0}\u0002", channel));
                    }
                    break;
            }
        }

        private void HandleJoinEvent(object sender, JoinChannelInfo info)
        {
            if (Enabled
                && !Bot.ServerConfig.ChannelBlacklist.Contains(info.Channel)
                && !Bot.ServerConfig.NickBlacklist.Contains(info.Nick.Nickname)
                && !ChannelBlacklist.Contains(info.Channel)
                && !NickBlacklist.Contains(info.Nick.Nickname))
            {
                ProcessAutoBan(info.Channel, info.Nick.Nickname);
            }
        }

        private void HandleChannelMessageEvent(object sender, ChannelMessage info)
        {
            if (Enabled
                && !Bot.ServerConfig.ChannelBlacklist.Contains(info.Channel)
                && !Bot.ServerConfig.NickBlacklist.Contains(info.Sender.Nickname)
                && !ChannelBlacklist.Contains(info.Channel)
                && !NickBlacklist.Contains(info.Sender.Nickname))
            {
                ProcessAutoBan(info.Channel, info.Sender.Nickname);
            }
        }

        private void HandleChannelNoticeEvent(object sender, ChannelNotice info)
        {
            if (Enabled
                && !Bot.ServerConfig.ChannelBlacklist.Contains(info.Channel)
                && !Bot.ServerConfig.NickBlacklist.Contains(info.Sender.Nickname)
                && !ChannelBlacklist.Contains(info.Channel)
                && !NickBlacklist.Contains(info.Sender.Nickname))
            {
                ProcessAutoBan(info.Channel, info.Sender.Nickname);
            }
        }

        private void ModifyUserPrivilege(bool set, CommandMessage command, ChannelMode mode)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            if (Bot.CheckChannelAccess(channel, command.Nick.Nickname, Bot.ChannelModeMapping[mode]))
            {
                SetMode(set, channel, mode, command.Arguments["Nickname"]);
            }
            else
            {
                string noAccessMessage = string.Format("You do not have access to set mode \u0002+{0}\u000F for \u0002{1}\u000F on \u0002{2}\u000F.", mode, command.Arguments["Nickname"], channel);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage, true);
            }
        }

        private void ModifyAutoUserPrivilege(string optionCommand, CommandMessage command, ChannelMode mode)
        {
            bool set = true;
            if (command.Arguments["Option"].ToLower() == "del")
            {
                set = false;
            }
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            if (Bot.CheckChannelAccess(channel, command.Nick.Nickname, Bot.ChannelModeMapping[mode]))
            {
                SetMode(set, channel, mode, command.Arguments["Nickname"]);
                Bot.IRC.Command.SendPrivateMessage("ChanServ", string.Format("{0} {1} {2} {3}", optionCommand, channel, command.Arguments["Option"], command.Arguments["Nickname"]));
            }
            else
            {
                string noAccessMessage = string.Format("You do not have access to \u0002{0}\u000F \u0002{1}\u000F to the \u0002{2}\u000F list on \u0002{3}\u000F.", command.Arguments["Option"].ToLower(), command.Arguments["Nickname"], optionCommand, channel);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage, true);
            }
        }

        private void ModifyChannelModes(Command curCommand, CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            bool allowedMode = true;
            bool allowedCommand = Bot.CheckChannelAccess(channel, command.Nick.Nickname, curCommand.AllowedAccess);
            if (allowedCommand)
            {
                List<ChannelModeInfo> modeList = new List<ChannelModeInfo>();
                if (command.Arguments.ContainsKey("Parameters"))
                {
                    modeList = Bot.IRC.ParseChannelModeString(command.Arguments["Modes"],
                        command.Arguments["Parameters"]);
                }
                else
                {
                    modeList = Bot.IRC.ParseChannelModeString(command.Arguments["Modes"], string.Empty);
                }
                ChannelMode mode = ChannelMode.q;
                for (int i = 0; i < modeList.Count; i++)
                {
                    switch (modeList[i].Mode)
                    {
                        case ChannelMode.v:
                        case ChannelMode.h:
                        case ChannelMode.o:
                        case ChannelMode.a:
                        case ChannelMode.q:
                            allowedMode = Bot.CheckChannelAccess(channel, command.Nick.Nickname, Bot.ChannelModeMapping[modeList[i].Mode]);
                            if (!allowedMode)
                            {
                                mode = modeList[i].Mode;
                            }
                            break;
                    }
                }
                if (allowedMode)
                {
                    Bot.IRC.Command.SendMode(channel, modeList);
                }
                else
                {
                    string noAccessMessage = string.Format("You do not have access to set mode \u0002+{0}\u000F on \u0002{1}\u000F.", mode, channel);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage, true);
                }
            }
            else
            {
                string noAccessMessage = string.Format("You do not have access to use \u0002{0}\u000F on \u0002{1}\u000F.", command.Command, channel);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage, true);
            }
        }

        private void SetMode(bool set, string channel, ChannelMode mode, string nickname)
        {
            ChannelModeInfo modeInfo = new ChannelModeInfo();
            modeInfo.Mode = mode;
            modeInfo.Parameter = nickname;
            modeInfo.Set = set;
            Bot.IRC.Command.SendMode(channel, modeInfo);
        }

        private void SetMode(bool set, string channel, ChannelMode mode, List<string> nicknames)
        {
            List<ChannelModeInfo> modeInfos = new List<ChannelModeInfo>();
            foreach (var nickname in nicknames)
            {
                ChannelModeInfo modeInfo = new ChannelModeInfo();
                modeInfo.Mode = mode;
                modeInfo.Parameter = nickname;
                modeInfo.Set = set;
                modeInfos.Add(modeInfo);
            }
            Bot.IRC.Command.SendMode(channel, modeInfos);
        }

        private void ModifyChannelTopic(Command curCommand, CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            if (Bot.CheckChannelAccess(channel, command.Nick.Nickname, curCommand.AllowedAccess))
            {
                Bot.IRC.Command.SendTopic(channel, command.Arguments["Message"]);
            }
            else
            {
                string noAccessMessage = string.Format("You do not have access to change the topic on \u0002{0}\u000F.", channel);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage, true);
            }
        }

        private void InviteNick(Command curCommand, CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            if (Bot.CheckChannelAccess(channel, command.Nick.Nickname, curCommand.AllowedAccess))
            {
                Bot.IRC.Command.SendInvite(channel, command.Arguments["Nickname"]);
            }
            else
            {
                string noAccessMessage = string.Format("You do not have access to invite someone to \u0002{0}\u000F.", channel);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage, true);
            }
        }

        private void AutoBan(Command curCommand, CommandMessage command)
        {
            int timeToBan = 1;
            int.TryParse(GetOptionValue("Seconds To Ban").ToString(), out timeToBan);
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            bool hasNick = command.Arguments.ContainsKey("Nickname");
            string nickname = hasNick ? command.Arguments["Nickname"] : string.Empty;
            string method = command.Arguments["Method"];
            if (Bot.CheckChannelAccess(channel, command.Nick.Nickname, curCommand.AllowedAccess) && (!hasNick || Bot.CheckNickAccess(channel, command.Nick.Nickname, nickname)))
            {
                command.Arguments.Add("Time", timeToBan.ToString());
                switch (method.ToLower())
                {
                    case "add":
                        // Set the auto ban
                        AddAutoBan(command);

                        // Process the auto ban now that we added it
                        if (command.Arguments.ContainsKey("Reason"))
                        {
                            command.Arguments["Reason"] = string.Format("[Auto Ban] {0}", command.Arguments["Reason"].ToString());
                        }
                        else
                        {
                            command.Arguments.Add("Reason", "[Auto Ban] No Reason Specified");
                        }
                        TimedBan(curCommand, command);
                        KickNick(curCommand, command);
                        break;
                    case "delete":
                    case "del":
                        // Remove the auto-ban
                        DeleteAutoBan(command);

                        // Force an unban in case they are still banned
                        BanNick(false, curCommand, command);
                        break;
                    case "view":
                        ViewAutoBan(command);
                        break;
                }
            }
            else
            {
                string noAccessMessage = string.Format("You do not have access to {0} an auto ban for \u0002{1}\u000F on \u0002{2}\u000F.", method, command.Arguments["Nickname"], channel);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage, true);
            }
        }

        private void BanNick(bool set, Command curCommand, CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            if (Bot.CheckChannelAccess(channel, command.Nick.Nickname, curCommand.AllowedAccess) && Bot.CheckNickAccess(channel, command.Nick.Nickname, command.Arguments["Nickname"]))
            {
                string banMask = command.Arguments["Nickname"];
                if (!banMask.Contains("@") || !banMask.Contains("!"))
                {
                    string search = "SELECT `nickinfo`.`username`, `nickinfo`.`host`, `nicks`.`nickname` FROM `nickinfo` " +
                                    "INNER JOIN `nicks` " +
                                    "ON `nickinfo`.`nick_id` = `nicks`.`id` " +
                                    "INNER JOIN `servers` " +
                                    "ON `nicks`.`server_id` = `servers`.`id` " +
                                    "WHERE `servers`.`name` = {0} AND `nicks`.`nickname` = {1}";
                    List<Dictionary<string, object>> results = Bot.Database.Query(search, new object[] {Bot.ServerConfig.Name, banMask});

                    if (results.Any())
                    {
                        List<string> banMasks = new List<string>();
                        foreach (Dictionary<string, object> result in results)
                        {
                            var nickname = result["nickname"].ToString();
                            var host = result["host"].ToString();
                            var username = result["username"].ToString();
                            if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(username))
                            {
                                banMask = string.Format("*!*{0}@{1}", username, host);
                            }
                            else if (!string.IsNullOrEmpty(host))
                            {
                                banMask = string.Format("*!*@{0}", host);
                            }
                            else if (!string.IsNullOrEmpty(username))
                            {
                                banMask = string.Format("{0}!*{1}@*", nickname, username);
                            }
                            else
                            {
                                banMask = string.Format("{0}!*@*", nickname);
                            }
                            banMasks.Add(banMask);
                        }
                        SetMode(set, channel, ChannelMode.b, banMasks);
                    }
                    else
                    {
                        SetMode(set, channel, ChannelMode.b, string.Format("{0}!*@*", banMask));
                    }
                }
                else
                {
                    SetMode(set, channel, ChannelMode.b, banMask);
                }
            }
            else
            {
                string banMessage = "ban";
                if (!set)
                {
                    banMessage = "unban";
                }
                string noAccessMessage = string.Format("You do not have access to {0} \u0002{1}\u000F on \u0002{2}\u000F.", banMessage, command.Arguments["Nickname"], channel);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage, true);
            }
        }

        private void TimedBan(Command curCommand, CommandMessage command)
        {
            double timeout;
            if (double.TryParse(command.Arguments["Time"], out timeout) && timeout >= 0)
            {
                BanNick(true, curCommand, command);
                Timer unban_trigger = new Timer();
                unban_trigger.Interval = (timeout * 1000.0);
                unban_trigger.Enabled = true;
                unban_trigger.AutoReset = false;
                unban_trigger.Elapsed += (sender, e) => TimedUnBan(sender, e, curCommand, command);
                listLock.EnterWriteLock();
                unbanTimers.Add(unban_trigger);
                listLock.ExitWriteLock();
            }
            else
            {
                string notValid = "Please enter a valid time.";
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, notValid, true);
            }
        }

        private void TimedUnBan(object sender, EventArgs e, Command curCommand, CommandMessage command)
        {
            Timer unbanTimer = (Timer)sender;
            unbanTimer.Enabled = false;
            BanNick(false, curCommand, command);
            listLock.EnterWriteLock();
            unbanTimers.Remove(unbanTimer);
            listLock.ExitWriteLock();
        }

        private void KickNick(Command curCommand, CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            if (Bot.CheckChannelAccess(channel, command.Nick.Nickname, curCommand.AllowedAccess) && Bot.CheckNickAccess(channel, command.Nick.Nickname, command.Arguments["Nickname"]))
            {
                if (command.Arguments.ContainsKey("Reason"))
                {
                    Bot.IRC.Command.SendKick(channel, command.Arguments["Nickname"], command.Arguments["Reason"]);
                }
                else
                {
                    Bot.IRC.Command.SendKick(channel, command.Arguments["Nickname"]);
                }
            }
            else
            {
                string noAccessMessage = string.Format("You do not have access to kick \u0002{0}\u000F from \u0002{1}\u000F.", command.Arguments["Nickname"], channel);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage, true);
            }
        }

        private void KickSelf(CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            if (command.Arguments.ContainsKey("Reason"))
            {
                Bot.IRC.Command.SendKick(channel, command.Nick.Nickname, command.Arguments["Reason"]);
            }
            else
            {
                Bot.IRC.Command.SendKick(channel, command.Nick.Nickname);
            }
        }

        private void ClearChannel(Command curCommand, CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            if (Bot.CheckChannelAccess(channel, command.Nick.Nickname, curCommand.AllowedAccess))
            {
                Bot.IRC.Command.SendPrivateMessage("ChanServ", string.Format("CLEAR {0} {1}", channel, command.Arguments["Target"]));
            }
            else
            {
                string noAccessMessage = string.Format("You do not have access to clear \u0002{0}\u000F on \u0002{1}\u000F.", command.Arguments["Target"], channel);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage, true);
            }
        }

        private void ProcessAutoBan(string channel, string nickname)
        {
            int timeToBan = 1;
            int.TryParse(GetOptionValue("Seconds To Ban").ToString(), out timeToBan);
            // Handle Auto Bans
            List<Dictionary<string, object>> results = GetAutoBanList(channel, nickname);
            if (results.Any())
            {
                foreach (Dictionary<string, object> result in results)
                {
                    // temp ban/kick them
                    CommandMessage newMsg = new CommandMessage();
                    newMsg.Arguments.Add("Channel", channel);
                    newMsg.Arguments.Add("Nickname", nickname);
                    newMsg.Arguments.Add("Time", timeToBan.ToString());
                    newMsg.Arguments.Add("Reason", string.Format("[Auto Ban] {0}", result["Reason"].ToString()));
                    newMsg.Nick = new Nick() { Nickname = Bot.IRC.Nickname };
                    Command foundCommand = Commands.Find(c => c.Name == "Auto Ban");
                    TimedBan(foundCommand, newMsg);
                    KickNick(foundCommand, newMsg);
                }
            }
        }

        private void AddAutoBan(CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            string nickname = command.Arguments["Nickname"];
            string reason = command.Arguments.ContainsKey("Reason") ? command.Arguments["Reason"] : "No Reason Specified";
            List<Dictionary<string, object>> results = GetAutoBanList(channel, nickname);

            if (!results.Any())
            {
                AddChannel(channel);
                AddNick(command.Nick);
                AddNick(new Nick() { Nickname = nickname });
                string query = "INSERT INTO `autobans` SET " +
                                "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                                "`channel_id` = (SELECT `channels`.`id` FROM `channels` INNER JOIN `servers` ON `servers`.`id` = `channels`.`server_id` WHERE `servers`.`name` = {1} && `channels`.`name` = {2}), " +
                                "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {3} && `nickname` = {4}), " +
                                "`request_nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {5} && `nickname` = {6}), " +
                                "`reason` = {7}, " +
                                "`date_added` = {8}";
                Bot.Database.Execute(query, new object[] { Bot.ServerConfig.Name, Bot.ServerConfig.Name, channel, Bot.ServerConfig.Name, nickname, Bot.ServerConfig.Name, command.Nick.Nickname, reason, command.TimeStamp });
                string introMessage = string.Format("Added Auto Ban for {0}.", nickname);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, introMessage);
            }
            else
            {
                string maxMessage = string.Format("There is already an Auto Ban set for {0}.", command.Nick.Nickname);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, maxMessage, true);
            }
        }

        private void DeleteAutoBan(CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            string nickname = command.Arguments["Nickname"];
            List<Dictionary<string, object>> results = GetAutoBanList(channel, nickname);

            if (results.Any())
            {
                foreach (Dictionary<string, object> result in results)
                {
                    int id = Convert.ToInt32(result["BanID"]);
                    string query = "DELETE FROM `autobans` " +
                                    "WHERE `id` = {0}";
                    Bot.Database.Execute(query, new object[] { id });
                    string introMessage = string.Format("Auto Ban has been deleted for: {0}", nickname);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, introMessage);
                }
            }
            else
            {
                string invalid = string.Format("No Auto Ban exists for: {0}", nickname);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid, true);
            }
        }

        private void ViewAutoBan(CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;

            List<Dictionary<string, object>> results = command.Arguments.ContainsKey("Nickname") ? GetAutoBanList(channel, command.Arguments["Nickname"]) : GetAutoBanList(channel);
            if (results.Any())
            {
                for (int i = 0; i < results.Count; i++)
                {
                    int nickID = 0;
                    int requestID = 0;
                    int.TryParse(results[i]["NickID"].ToString(), out nickID);
                    int.TryParse(results[i]["RequestID"].ToString(), out requestID);
                    DateTime addedTime = DateTime.Now;
                    DateTime.TryParse(results[i]["DateAdded"].ToString(), out addedTime);
                    string introMessage = string.Format("Auto Ban #\u0002{0}\u0002 by {1} on {2} for reason: {3}", GetNickname(nickID), GetNickname(requestID), addedTime.ToString("G"), results[i]["Reason"]);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, introMessage, true);
                }
            }
            else
            {
                string invalid = "No Auto Bans Available.";
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid, true);
            }
        }

        private List<Dictionary<string, object>> GetAutoBanList(string channel, string nickname)
        {
            // Check to see if they have reached the max number of introductions
            string search = "SELECT `autobans`.`id` AS `BanID`, `autobans`.`nick_id` AS `NickID`, `autobans`.`request_nick_id` AS `RequestID`, `autobans`.`reason` AS `Reason`, `autobans`.`date_added` AS `DateAdded` FROM `autobans` " +
                            "INNER JOIN `nicks` " +
                            "ON `autobans`.`nick_id` = `nicks`.`id` " +
                            "INNER JOIN `channels` " +
                            "ON `autobans`.`channel_id` = `channels`.`id` " +
                            "INNER JOIN `servers` " +
                            "ON `autobans`.`server_id` = `servers`.`id` " +
                            "WHERE `servers`.`name` = {0} AND `channels`.`name` = {1} AND `nicks`.`nickname` = {2}";
            return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, channel, nickname });
        }

        private List<Dictionary<string, object>> GetAutoBanList(string channel)
        {
            // Check to see if they have reached the max number of introductions
            string search = "SELECT `autobans`.`id` AS `BanID`, `autobans`.`nick_id` AS `NickID`, `autobans`.`request_nick_id` AS `RequestID`, `autobans`.`reason` AS `Reason`, `autobans`.`date_added` AS `DateAdded` FROM `autobans` " +
                            "INNER JOIN `channels` " +
                            "ON `autobans`.`channel_id` = `channels`.`id` " +
                            "INNER JOIN `servers` " +
                            "ON `autobans`.`server_id` = `servers`.`id` " +
                            "WHERE `servers`.`name` = {0} AND `channels`.`name` = {1}";
            return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, channel });
        }
    }
}
