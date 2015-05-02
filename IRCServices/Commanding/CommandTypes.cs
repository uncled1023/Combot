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

        public PrivateMessageCommand()
        {
            Recipient = string.Empty;
            Message = string.Empty;
        }
    }

    public class PrivateNoticeCommand : ICommand
    {
        public string Recipient { get; set; }
        public string Message { get; set; }

        public PrivateNoticeCommand()
        {
            Recipient = string.Empty;
            Message = string.Empty;
        }
    }

    public class CTCPMessageCommand : ICommand
    {
        public string Recipient { get; set; }
        public string Command { get; set; }
        public string Arguments { get; set; }

        public CTCPMessageCommand()
        {
            Recipient = string.Empty;
            Command = string.Empty;
            Arguments = string.Empty;
        }
    }

    public class CTCPNoticeCommand : ICommand
    {
        public string Recipient { get; set; }
        public string Command { get; set; }
        public string Arguments { get; set; }

        public CTCPNoticeCommand()
        {
            Recipient = string.Empty;
            Command = string.Empty;
            Arguments = string.Empty;
        }
    }

    public class PasswordCommand : ICommand
    {
        public string Password { get; set; }

        public PasswordCommand()
        {
            Password = string.Empty;
        }
    }

    public class NickCommand : ICommand
    {
        public string Nick { get; set; }

        public NickCommand()
        {
            Nick = string.Empty;
        }
    }

    public class UserCommand : ICommand
    {
        public string Username { get; set; }
        public string Hostname { get; set; }
        public string Servername { get; set; }
        public string Realname { get; set; }

        public UserCommand()
        {
            Username = string.Empty;
            Hostname = string.Empty;
            Hostname = string.Empty;
            Realname = string.Empty;
        }
    }

    public class OperCommand : ICommand
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public OperCommand()
        {
            Username = string.Empty;
            Password = string.Empty;
        }
    }

    public class QuitCommand : ICommand
    {
        public string Message { get; set; }

        public QuitCommand()
        {
            Message = string.Empty;
        }
    }

    public class JoinCommand : ICommand
    {
        public string Channel { get; set; }
        public string Key { get; set; }

        public JoinCommand()
        {
            Channel = string.Empty;
            Key = string.Empty;
        }
    }

    public class PartCommand : ICommand
    {
        public string Channel { get; set; }

        public PartCommand()
        {
            Channel = string.Empty;
        }
    }

    public class ChannelModeCommand : ICommand
    {
        public string Channel { get; set; }
        public ChannelModeInfo Mode { get; set; }

        public ChannelModeCommand()
        {
            Channel = string.Empty;
            Mode = new ChannelModeInfo();
        }
    }

    public class UserModeCommand : ICommand
    {
        public string Nick { get; set; }
        public UserModeInfo Mode { get; set; }

        public UserModeCommand()
        {
            Nick = string.Empty;
            Mode = new UserModeInfo();
        }
}

    public class TopicCommand : ICommand
    {
        public string Channel { get; set; }
        public string Topic { get; set; }

        public TopicCommand()
        {
            Channel = string.Empty;
            Topic = string.Empty;
        }
    }

    public class NamesCommand : ICommand
    {
        public string Channel { get; set; }

        public NamesCommand()
        {
            Channel = string.Empty;
        }
    }

    public class ListCommand : ICommand
    {
        public string Channel { get; set; }

        public ListCommand()
        {
            Channel = string.Empty;
        }
    }

    public class InviteCommand : ICommand
    {
        public string Channel { get; set; }
        public string Nick { get; set; }

        public InviteCommand()
        {
            Channel = string.Empty;
            Nick = string.Empty;
        }
    }

    public class KickCommand : ICommand
    {
        public string Channel { get; set; }
        public string Nick { get; set; }
        public string Reason { get; set; }

        public KickCommand()
        {
            Channel = string.Empty;
            Nick = string.Empty;
            Reason = string.Empty;
        }
    }

    public class VersionCommand : ICommand
    {
        public string Server { get; set; }

        public VersionCommand()
        {
            Server = string.Empty;
        }
    }

    public class StatsCommand : ICommand
    {
        public string Stat { get; set; }
        public string Parameter { get; set; }

        public StatsCommand()
        {
            Stat = string.Empty;
            Parameter = string.Empty;
        }
    }

    public class LinksCommand : ICommand
    {
        public string Mask { get; set; }
        public string Server { get; set; }

        public LinksCommand()
        {
            Mask = string.Empty;
            Server = string.Empty;
        }
    }

    public class TimeCommand : ICommand
    {
        public string Server { get; set; }

        public TimeCommand()
        {
            Server = string.Empty;
        }
    }

    public class ConnectCommand : ICommand
    {
        public string Originator { get; set; }
        public int Port { get; set; }
        public string Server { get; set; }

        public ConnectCommand()
        {
            Originator = string.Empty;
            Port = 0;
            Server = string.Empty;
        }
    }

    public class TraceCommand : ICommand
    {
        public string Target { get; set; }

        public TraceCommand()
        {
            Target = string.Empty;
        }
    }

    public class AdminCommand : ICommand
    {
        public string Host { get; set; }

        public AdminCommand()
        {
            Host = string.Empty;
        }
    }

    public class InfoCommand : ICommand
    {
        public string Host { get; set; }

        public InfoCommand()
        {
            Host = string.Empty;
        }
    }

    public class WhoCommand : ICommand
    {
        public string Host { get; set; }

        public WhoCommand()
        {
            Host = string.Empty;
        }
    }

    public class WhoisCommand : ICommand
    {
        public string Server { get; set; }
        public string Nick { get; set; }

        public WhoisCommand()
        {
            Server = string.Empty;
            Nick = string.Empty;
        }
    }

    public class WhowasCommand : ICommand
    {
        public string Nick { get; set; }
        public int Entries { get; set; }
        public string Server { get; set; }

        public WhowasCommand()
        {
            Nick = string.Empty;
            Entries = 0;
            Server = string.Empty;
        }
    }

    public class KillCommand : ICommand
    {
        public string Nick { get; set; }
        public string Comment { get; set; }

        public KillCommand()
        {
            Nick = string.Empty;
            Comment = string.Empty;
        }
    }

    public class PingCommand : ICommand
    {
        public string Recipient { get; set; }

        public PingCommand()
        {
            Recipient = string.Empty;
        }
    }

    public class PongCommand : ICommand
    {
        public string Sender { get; set; }
        public string Recipient { get; set; }

        public PongCommand()
        {
            Sender = string.Empty;
            Recipient = string.Empty;
        }
    }

    public class AwayCommand : ICommand
    {
        public string Message { get; set; }

        public AwayCommand()
        {
            Message = string.Empty;
        }
    }

    public class RehashCommand : ICommand
    {
        public RehashCommand() { }
    }

    public class RestartCommand : ICommand
    {
        public RestartCommand() { }
    }

    public class SummonCommand : ICommand
    {
        public string Nick { get; set; }
        public string Host { get; set; }

        public SummonCommand()
        {
            Nick = string.Empty;
            Host = string.Empty;
        }
    }

    public class UsersCommand : ICommand
    {
        public string Server { get; set; }

        public UsersCommand()
        {
            Server = string.Empty;
        }
    }

    public class WallopsCommand : ICommand
    {
        public string Message { get; set; }

        public WallopsCommand()
        {
            Message = string.Empty;
        }
    }

    public class UserhostCommand : ICommand
    {
        public string Nicks { get; set; }

        public UserhostCommand()
        {
            Nicks = string.Empty;
        }
    }

    public class IsonCommand : ICommand
    {
        public string Nicks { get; set; }

        public IsonCommand()
        {
            Nicks = string.Empty;
        }
    }
}