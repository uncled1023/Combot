using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Combot.IRCServices.Messaging;

namespace Combot.Modules.ModuleClasses
{
    public class Version : Module
    {
        private List<VersionItem> versionList;
        private ReaderWriterLockSlim listLock;

        public override void Initialize()
        {
            listLock = new ReaderWriterLockSlim();
            versionList = new List<VersionItem>();
            Bot.IRC.Message.CTCPMessageRecievedEvent += HandleVersionQuery;
            Bot.IRC.Message.CTCPNoticeRecievedEvent += HandleVersionResponse;
            Bot.CommandReceivedEvent += HandleCommandEvent;
        }

        public override void ParseCommand(CommandMessage command)
        {
            if (Commands.Find(cmd => cmd.Name == "Version Check").Triggers.Contains(command.Command))
            {
                VersionItem tmpItem = new VersionItem();
                tmpItem.Location = command.Location;
                tmpItem.LocationType = command.LocationType;
                tmpItem.Nick = command.Arguments["Nickname"];
                listLock.EnterWriteLock();
                if (versionList.Exists(item => item.Nick == command.Arguments["Nickname"]))
                {
                    versionList.RemoveAll(item => item.Nick == command.Arguments["Nickname"]);
                }
                versionList.Add(tmpItem);
                listLock.ExitWriteLock();
                Bot.IRC.SendCTCPMessage(command.Arguments["Nickname"], "VERSION");
            }
        }

        public void HandleVersionQuery(object sender, CTCPMessage message)
        {
            if (message.Command.ToLower() == "version")
            {
                Bot.IRC.SendCTCPNotice(message.Sender.Nickname, "VERSION", string.Format("Combot v{0} on {1}", Assembly.GetExecutingAssembly().GetName().Version, GetOptionValue("Machine Reply")));
            }
        }

        public void HandleVersionResponse(object sender, CTCPMessage message)
        {
            if (message.Command == "VERSION")
            {
                listLock.EnterReadLock();
                VersionItem versionItem = versionList.Find(item => item.Nick == message.Sender.Nickname);
                listLock.ExitReadLock();
                if (versionItem != null)
                {
                    switch (versionItem.LocationType)
                    {
                        case LocationType.Channel:
                            Bot.IRC.SendPrivateMessage(versionItem.Location, string.Format("[{0}] Using version: {1}", versionItem.Nick, message.Arguments));
                            break;
                        case LocationType.Query:
                            Bot.IRC.SendPrivateMessage(versionItem.Nick, string.Format("[{0}] Using version: {1}", versionItem.Nick, message.Arguments));
                            break;
                        case LocationType.Notice:
                            Bot.IRC.SendNotice(versionItem.Nick, string.Format("[{0}] Using version: {1}", versionItem.Nick, message.Arguments));
                            break;
                    }
                    listLock.EnterWriteLock();
                    versionList.RemoveAll(item => item.Nick == versionItem.Nick);
                    listLock.ExitWriteLock();
                }
            }
        }
    }

    public class VersionItem
    {
        public string Nick { get; set; }
        public string Location { get; set; }
        public LocationType LocationType { get; set; }

        public VersionItem()
        {
            Nick = string.Empty;
            Location = string.Empty;
            LocationType = LocationType.Channel;
        }
    }
}