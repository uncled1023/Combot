using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Combot.IRCServices;
using Combot.IRCServices.Messaging;

namespace Combot.Modules.Plugins
{
    public class Ping_Me : Module
    {
        private List<PingItem> pingList;
        private ReaderWriterLockSlim listLock;

        public override void Initialize()
        {
            listLock = new ReaderWriterLockSlim();
            pingList = new List<PingItem>();
            Bot.IRC.Message.CTCPNoticeReceivedEvent += HandlePingResponse;
            Bot.CommandReceivedEvent += HandleCommandEvent;
        }

        public override void ParseCommand(CommandMessage command)
        {
            Command foundCommand = Commands.Find(c => c.Triggers.Contains(command.Command));

            if (foundCommand.Name == "Ping Me")
            {
                int epoch = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                PingItem tmpItem = new PingItem();
                tmpItem.Nick = command.Nick.Nickname;
                tmpItem.Location = command.Location;
                tmpItem.MessageType = command.MessageType;
                tmpItem.Timestamp = DateTime.Now;
                listLock.EnterWriteLock();
                if (pingList.Exists(item => item.Nick == command.Nick.Nickname))
                {
                    pingList.RemoveAll(item => item.Nick == command.Nick.Nickname);
                }
                pingList.Add(tmpItem);
                listLock.ExitWriteLock();
                Bot.IRC.Command.SendCTCPMessage(command.Nick.Nickname, "PING", epoch.ToString());
            }
        }

        private void HandlePingResponse(object sender, CTCPMessage e)
        {
            if (e.Command == "PING")
            {
                listLock.EnterReadLock();
                PingItem pingItem = pingList.Find(item => item.Nick == e.Sender.Nickname);
                listLock.ExitReadLock();
                if (pingItem != null)
                {
                    DateTime curTime = DateTime.Now;
                    DateTime prevTime = pingItem.Timestamp;
                    TimeSpan difTime = curTime.Subtract(prevTime);
                    string timeString = string.Empty;
                    if (difTime.Days > 0)
                    {
                        timeString += difTime.Days.ToString() + " Days, ";
                    }
                    if (difTime.Hours > 0)
                    {
                        timeString += difTime.Hours.ToString() + " Hours, ";
                    }
                    if (difTime.Minutes > 0)
                    {
                        timeString += difTime.Minutes.ToString() + " Minutes, ";
                    }
                    if (difTime.Seconds > 0)
                    {
                        timeString += difTime.Seconds.ToString() + " Seconds, ";
                    }
                    if (difTime.Milliseconds > 0)
                    {
                        timeString += difTime.Milliseconds.ToString() + " Milliseconds";
                    }
                    switch (pingItem.MessageType)
                    {
                        case MessageType.Channel:
                            Bot.IRC.Command.SendPrivateMessage(pingItem.Location, string.Format("{0}, your ping is {1}", pingItem.Nick, timeString));
                            break;
                        case MessageType.Notice:
                            Bot.IRC.Command.SendNotice(pingItem.Nick, string.Format("Your ping is {0}", timeString));
                            break;
                        case MessageType.Query:
                            Bot.IRC.Command.SendPrivateMessage(pingItem.Nick, string.Format("Your ping is {0}", timeString));
                            break;
                    }
                    listLock.EnterWriteLock();
                    pingList.RemoveAll(item => item.Nick == pingItem.Nick);
                    listLock.ExitWriteLock();
                }
            }
        }

        private class PingItem
        {
            public string Nick { get; set; }
            public string Location { get; set; }
            public MessageType MessageType { get; set; }
            public DateTime Timestamp { get; set; }

            public PingItem()
            {
                Nick = string.Empty;
                Location = string.Empty;
                MessageType = MessageType.Channel;
                Timestamp = DateTime.Now;
            }
        }
    }
}