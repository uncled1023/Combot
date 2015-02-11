using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Combot.IRCServices.Messaging;

namespace Combot.Modules.Plugins
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
            Command foundCommand = Commands.Find(c => c.Triggers.Contains(command.Command));

            if (foundCommand.Name == "Version Check")
            {
                string nick = command.Arguments["Nickname"].ToString();
                List<string> nickList = nick.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                for (int i = 0; i < nickList.Count; i++)
                {
                    VersionItem tmpItem = new VersionItem();
                    tmpItem.Location = command.Location;
                    tmpItem.MessageType = command.MessageType;
                    tmpItem.Nick = nickList[i];
                    listLock.EnterWriteLock();
                    if (versionList.Exists(item => item.Nick == nickList[i]))
                    {
                        versionList.RemoveAll(item => item.Nick == nickList[i]);
                    }
                    versionList.Add(tmpItem);
                    listLock.ExitWriteLock();
                    Bot.IRC.SendCTCPMessage(nickList[i], "VERSION");
                }
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
                VersionItem versionItem = versionList.Find(item => item.Nick.ToLower() == message.Sender.Nickname.ToLower());
                listLock.ExitReadLock();
                if (versionItem != null)
                {
                    string verResponse = string.Format("[{0}] Using version: {1}", versionItem.Nick, message.Arguments);
                    SendResponse(versionItem.MessageType, versionItem.Location, message.Sender.Nickname, verResponse);
                    listLock.EnterWriteLock();
                    versionList.RemoveAll(item => item.Nick == versionItem.Nick);
                    listLock.ExitWriteLock();
                }
            }
        }

        private class VersionItem
        {
            public string Nick { get; set; }
            public string Location { get; set; }
            public MessageType MessageType { get; set; }

            public VersionItem()
            {
                Nick = string.Empty;
                Location = string.Empty;
                MessageType = MessageType.Channel;
            }
        }
    }
}
