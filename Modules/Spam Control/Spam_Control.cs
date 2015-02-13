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

        public override void Initialize()
        {
            SpamMessageList = new List<SpamMessageInfo>();
            SpamHighlightList = new List<SpamHighlightInfo>();
            SpamMessageLock = new ReaderWriterLockSlim();
            SpamHighlightLock = new ReaderWriterLockSlim();
            Bot.IRC.Message.ChannelMessageReceivedEvent += HandleChannelMessage;
        }

        private void HandleChannelMessage(object sender, ChannelMessage message)
        {
            if (!ChannelBlacklist.Contains(message.Channel) && !NickBlacklist.Contains(message.Sender.Nickname))
            {
                int timeThreshold = Convert.ToInt32(GetOptionValue("Time Threshold"));
                int maxMessages = Convert.ToInt32(GetOptionValue("Max Messages"));
                int maxHighlights = Convert.ToInt32(GetOptionValue("Max Highlights"));
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
                            Bot.IRC.Command.SendKick(info.Channel, info.Nick, string.Format("Please do not spam.  You have messaged {0} times within {1}ms.", info.Lines, timeThreshold));
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

                // Check for highlight spam
                List<string> splitMessage = message.Message.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries).ToList();
                Channel channel = Bot.IRC.Channels.Find(chan => chan.Name == message.Channel);
                if (channel != null)
                {
                    List<string> foundNicks = splitMessage.FindAll(msg => channel.Nicks.Exists(nick => msg.Contains(nick.Nickname)));
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
                                Bot.IRC.Command.SendKick(info.Channel, info.Nick, string.Format("Please do not highlight spam.  You have highlighted {0} nicks within {1}ms.", info.Highlights, timeThreshold));
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
        }
    }
}
