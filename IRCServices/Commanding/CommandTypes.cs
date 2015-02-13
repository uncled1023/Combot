using System;
using System.Collections.Generic;

namespace Combot.IRCServices.Commanding
{
    abstract public class ICommand : EventArgs
    {
        public DateTime TimeStamp { get; set; }

        public ICommand()
        {
            TimeStamp = DateTime.Now;
        }
    }

    public class PrivateMessageCommand : ICommand
    {
        public string Recipient { get; set; }
        public string Message { get; set; }
    }

    public class PrivateNoticeCommand : ICommand
    {
        public string Recipient { get; set; }
        public string Message { get; set; }
    }

    public class CTCPMessageCommand : ICommand
    {
        public string Recipient { get; set; }
        public string Command { get; set; }
        public string Arguments { get; set; }
    }

    public class TopicCommand : ICommand
    {
        public string Channel { get; set; }
        public string Nick { get; set; }
        public string Topic { get; set; }
    }

    public class ChannelModeCommandInfo : ICommand
    {
        public string Channel { get; set; }
        public string Nick { get; set; }
        public List<ChannelModeInfo> Modes { get; set; }
    }

    public class UserModeCommandInfo : ICommand
    {
        public string Nick { get; set; }
        public List<UserModeInfo> Modes { get; set; }
    }

    public class NickCommandInfo : ICommand
    {
        public string OldNick { get; set; }
        public string NewNick { get; set; }
    }

    public class InviteCommandInfo : ICommand
    {
        public string Channel { get; set; }
        public string Recipient { get; set; }
    }

    public class JoinCommandInfo : ICommand
    {
        public string Channel { get; set; }
    }

    public class PartCommandInfo : ICommand
    {
        public string Channel { get; set; }
    }

    public class KickCommandInfo : ICommand
    {
        public string Channel { get; set; }
        public string Nick { get; set; }
        public string Reason { get; set; }
    }

    public class QuitCommandInfo : ICommand
    {
        public string Nick { get; set; }
        public string Message { get; set; }
    }

    public class PingCommandInfo : ICommand
    {
        public string Message { get; set; }
    }

    public class PongCommandInfo : ICommand
    {
        public string Message { get; set; }
    }
}