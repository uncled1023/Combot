using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Combot.IRCServices;
using Combot.IRCServices.Messaging;

namespace Combot.Modules.ModuleClasses
{
    public class PingMe : Module
    {
        private List<PingItem> pingList;
        private ReaderWriterLockSlim listLock;
 
        public override void Initialize()
        {
            listLock = new ReaderWriterLockSlim();
            pingList = new List<PingItem>();
            Bot.IRC.Message.CTCPNoticeRecievedEvent += HandlePingResponse;
            Bot.CommandReceivedEvent += HandleCommandEvent;
        }

        public override void ParseCommand(CommandMessage command)
        {
            if (Commands.Find(cmd => cmd.Name == "Ping Me").Triggers.Contains(command.Command))
            {
                int epoch = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                PingItem tmpItem = new PingItem();
                tmpItem.Nick = command.Nick.Nickname;
                tmpItem.Location = command.Location;
                tmpItem.LocationType = command.LocationType;
                tmpItem.Timestamp = DateTime.Now;
                listLock.EnterWriteLock();
                if (pingList.Exists(item => item.Nick == command.Nick.Nickname))
                {
                    pingList.RemoveAll(item => item.Nick == command.Nick.Nickname);
                }
                pingList.Add(tmpItem);
                listLock.ExitWriteLock();
                Bot.IRC.SendCTCP(command.Nick.Nickname, "PING", epoch.ToString());
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
                    switch (pingItem.LocationType)
                    {
                        case LocationType.Channel:
                            Bot.IRC.SendPrivateMessage(pingItem.Location, string.Format("{0}, your ping is {1}", pingItem.Nick, timeString));
                            break;
                        case LocationType.Notice:
                            Bot.IRC.SendNotice(pingItem.Nick, string.Format("Your ping is {0}", timeString));
                            break;
                        case LocationType.Query:
                            Bot.IRC.SendPrivateMessage(pingItem.Nick, string.Format("Your ping is {0}", timeString));
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
            public LocationType LocationType { get; set; }
            public DateTime Timestamp { get; set; }

            public PingItem()
            {
                Nick = string.Empty;
                Location = string.Empty;
                LocationType = LocationType.Channel;
                Timestamp = DateTime.Now;
            }
        }
    }
}