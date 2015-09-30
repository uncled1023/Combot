using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Combot.IRCServices;
using Combot.IRCServices.Messaging;

namespace Combot.Modules.Plugins
{
    public class Spam_Control : Module
    {
        private List<SpamMessageInfo> SpamMessageList;
        private List<SpamHighlightInfo> SpamHighlightList;
        private ReaderWriterLockSlim SpamMessageLock;
        private ReaderWriterLockSlim SpamHighlightLock;
        private List<System.Timers.Timer> unbanTimers;
        private ReaderWriterLockSlim unbanLock;
        private List<System.Timers.Timer> devoiceTimers;
        private ReaderWriterLockSlim devoiceLock;

        public override void Initialize()
        {
            unbanTimers = new List<System.Timers.Timer>();
            unbanLock = new ReaderWriterLockSlim();
            devoiceTimers = new List<System.Timers.Timer>();
            devoiceLock = new ReaderWriterLockSlim();
            SpamMessageList = new List<SpamMessageInfo>();
            SpamHighlightList = new List<SpamHighlightInfo>();
            SpamMessageLock = new ReaderWriterLockSlim();
            SpamHighlightLock = new ReaderWriterLockSlim();
            Bot.IRC.Message.ChannelMessageReceivedEvent += HandleChannelMessage;
        }

        private void HandleChannelMessage(object sender, ChannelMessage message)
        {
            if (Enabled && !ChannelBlacklist.Contains(message.Channel) && !NickBlacklist.Contains(message.Sender.Nickname))
            {
                CheckFlood(message);
                CheckHighlight(message);
            }
        }

        private void CheckFlood(ChannelMessage message)
        {
            int timeThreshold = Convert.ToInt32(GetOptionValue("Time Threshold"));
            int maxMessages = Convert.ToInt32(GetOptionValue("Max Messages"));
            int devoiceTime = Convert.ToInt32(GetOptionValue("Devoice Time"));
            int unbanTime = Convert.ToInt32(GetOptionValue("Unban Time"));
            bool voiceResponse = Convert.ToBoolean(GetOptionValue("Voice Response"));
            bool kickResponse = Convert.ToBoolean(GetOptionValue("Kick Response"));
            bool banResponse = Convert.ToBoolean(GetOptionValue("Ban Response"));
            bool unbanResponse = Convert.ToBoolean(GetOptionValue("Unban Response"));

            // Check for line spam
            if (SpamMessageList.Exists(msg => msg.Channel == message.Channel && msg.Nick == message.Sender.Nickname))
            {
                SpamMessageLock.EnterReadLock();
                SpamMessageInfo info = SpamMessageList.Find(msg => msg.Channel == message.Channel && msg.Nick == message.Sender.Nickname);
                SpamMessageLock.ExitReadLock();
                TimeSpan difference = message.TimeStamp.Subtract(info.FirstMessageTime);
                if (difference.TotalMilliseconds < timeThreshold)
                {
                    info.Lines++;
                    if (info.Lines > maxMessages)
                    {
                        if (voiceResponse)
                        {
                            TimedDeVoice(message, devoiceTime);
                        }
                        if (kickResponse)
                        {
                            Bot.IRC.Command.SendKick(info.Channel, info.Nick, string.Format("Please do not spam.  You have messaged {0} times within {1}ms.", info.Lines, timeThreshold));
                        }
                        if (banResponse)
                        {
                            if (unbanResponse)
                            {
                                TimedBan(message, unbanTime);
                            }
                            else
                            {
                                BanNick(true, message);
                            }
                        }
                        if (!kickResponse)
                        {
                            string spamMessage = string.Format("Please do not spam.  You have messaged {0} times within {1}ms.", info.Lines, timeThreshold);
                            SendResponse(MessageType.Channel, message.Channel, message.Sender.Nickname, spamMessage);
                        }
                        SpamMessageLock.EnterWriteLock();
                        SpamMessageList.Remove(info);
                        SpamMessageLock.ExitWriteLock();
                    }
                }
                else
                {
                    SpamMessageLock.EnterWriteLock();
                    info.Lines = 1;
                    info.FirstMessageTime = message.TimeStamp;
                    SpamMessageLock.ExitWriteLock();
                }
            }
            else
            {
                SpamMessageInfo info = new SpamMessageInfo();
                info.Channel = message.Channel;
                info.Nick = message.Sender.Nickname;
                info.Lines = 1;
                info.FirstMessageTime = message.TimeStamp;
                SpamMessageLock.EnterWriteLock();
                SpamMessageList.Add(info);
                SpamMessageLock.ExitWriteLock();
            }
        }

        private void CheckHighlight(ChannelMessage message)
        {
            int timeThreshold = Convert.ToInt32(GetOptionValue("Time Threshold"));
            int maxHighlights = Convert.ToInt32(GetOptionValue("Max Highlights"));
            int devoiceTime = Convert.ToInt32(GetOptionValue("Devoice Time"));
            int unbanTime = Convert.ToInt32(GetOptionValue("Unban Time"));
            bool voiceResponse = Convert.ToBoolean(GetOptionValue("Voice Response"));
            bool kickResponse = Convert.ToBoolean(GetOptionValue("Kick Response"));
            bool banResponse = Convert.ToBoolean(GetOptionValue("Ban Response"));
            bool unbanResponse = Convert.ToBoolean(GetOptionValue("Unban Response"));

            // Check for highlight spam
            List<string> splitMessage = message.Message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            Channel channel = Bot.IRC.Channels.Find(chan => chan.Name == message.Channel);
            if (channel != null)
            {
                List<string> foundNicks = splitMessage.FindAll(msg => channel.Nicks.Exists(nick => msg.Equals(nick.Nickname, StringComparison.InvariantCultureIgnoreCase)));
                if (foundNicks.Any())
                {
                    if (!SpamHighlightList.Exists(msg => msg.Channel == message.Channel && msg.Nick == message.Sender.Nickname))
                    {
                        SpamHighlightInfo newInfo = new SpamHighlightInfo();
                        newInfo.Channel = message.Channel;
                        newInfo.Nick = message.Sender.Nickname;
                        newInfo.Highlights = 0;
                        newInfo.FirstHighlightTime = message.TimeStamp;

                        SpamHighlightLock.EnterWriteLock();
                        SpamHighlightList.Add(newInfo);
                        SpamHighlightLock.ExitWriteLock();
                    }
                    SpamHighlightLock.EnterReadLock();
                    SpamHighlightInfo info = SpamHighlightList.Find(highlight => highlight.Channel == message.Channel && highlight.Nick == message.Sender.Nickname);
                    SpamHighlightLock.ExitReadLock();
                    TimeSpan difference = message.TimeStamp.Subtract(info.FirstHighlightTime);
                    if (difference.TotalMilliseconds < timeThreshold)
                    {
                        info.Highlights += foundNicks.Count;
                        if (info.Highlights > maxHighlights)
                        {
                            if (voiceResponse)
                            {
                                TimedDeVoice(message, devoiceTime);
                            }
                            if (kickResponse)
                            {
                                Bot.IRC.Command.SendKick(info.Channel, info.Nick, string.Format("Please do not highlight spam.  You have highlighted {0} nicks within {1}ms.", info.Highlights, timeThreshold));
                            }
                            if (banResponse)
                            {
                                if (unbanResponse)
                                {
                                    TimedBan(message, unbanTime);
                                }
                                else
                                {
                                    BanNick(true, message);
                                }
                            }
                            if (!kickResponse)
                            {
                                string spamMessage = string.Format("Please do not highlight spam.  You have highlighted {0} nicks within {1}ms.", info.Highlights, timeThreshold);
                                SendResponse(MessageType.Channel, message.Channel, message.Sender.Nickname, spamMessage);
                            }
                            SpamHighlightLock.EnterWriteLock();
                            SpamHighlightList.Remove(info);
                            SpamHighlightLock.ExitWriteLock();
                        }
                    }
                    else
                    {
                        SpamHighlightLock.EnterWriteLock();
                        info.Highlights = foundNicks.Count;
                        info.FirstHighlightTime = message.TimeStamp;
                        SpamHighlightLock.ExitWriteLock();
                    }
                }
            }
        }

        private void TimedDeVoice(ChannelMessage message, int timeout)
        {
            SetMode(false, message.Channel, ChannelMode.v, message.Sender.Nickname);
            System.Timers.Timer devoiceTrigger = new System.Timers.Timer();
            devoiceTrigger.Interval = (timeout * 1000.0);
            devoiceTrigger.Enabled = true;
            devoiceTrigger.AutoReset = false;
            devoiceTrigger.Elapsed += (sender, e) => TimedVoice(sender, e, message);
            devoiceLock.EnterWriteLock();
            devoiceTimers.Add(devoiceTrigger);
            devoiceLock.ExitWriteLock();
        }

        private void TimedVoice(object sender, EventArgs e, ChannelMessage message)
        {
            System.Timers.Timer devoiceTimer = (System.Timers.Timer)sender;
            devoiceTimer.Enabled = false;
            SetMode(true, message.Channel, ChannelMode.v, message.Sender.Nickname);
            devoiceLock.EnterWriteLock();
            devoiceTimers.Remove(devoiceTimer);
            devoiceLock.ExitWriteLock();
        }

        private void TimedBan(ChannelMessage message, int timeout)
        {
            BanNick(true, message);
            System.Timers.Timer unban_trigger = new System.Timers.Timer();
            unban_trigger.Interval = (timeout * 1000.0);
            unban_trigger.Enabled = true;
            unban_trigger.AutoReset = false;
            unban_trigger.Elapsed += (sender, e) => TimedUnBan(sender, e, message);
            unbanLock.EnterWriteLock();
            unbanTimers.Add(unban_trigger);
            unbanLock.ExitWriteLock();
        }

        private void TimedUnBan(object sender, EventArgs e, ChannelMessage message)
        {
            System.Timers.Timer unbanTimer = (System.Timers.Timer)sender;
            unbanTimer.Enabled = false;
            BanNick(false, message);
            unbanLock.EnterWriteLock();
            unbanTimers.Remove(unbanTimer);
            unbanLock.ExitWriteLock();
        }

        private void BanNick(bool set, ChannelMessage message)
        {
            string banMask = message.Sender.Nickname;
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
                    SetMode(set, message.Channel, ChannelMode.b, banMasks);
                }
                else
                {
                    SetMode(set, message.Channel, ChannelMode.b, string.Format("{0}!*@*", banMask));
                }
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
    }
}
