using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net;

namespace Combot
{
    public enum ErrorType
    {
        Bot = 0,
        TCP = 1,
        IRC = 2,
        Framework = 3
    }

    public enum MessageType
    {
        Service = 0,
        Channel = 1,
        Query = 2,
        Notice = 3,
        CTCP = 4
    }

    public class ChannelModeInfo
    {
        public ChannelMode Mode { get; set; }
        public bool Set { get; set; }
        public string Parameter { get; set; }
    }

    public class UserModeInfo
    {
        public UserMode Mode { get; set; }
        public bool Set { get; set; }
        public string Parameter { get; set; }
    }

    public enum ChannelMode
    {
        [Description("Admin")]
        a,
        [Description("Admin Only")]
        A,
        [Description("Ban")]
        b,
        [Description("Colourless")]
        c,
        [Description("No CTCP")]
        C,
        [Description("Ban Exempt")]
        e,
        [Description("Flood Protection")]
        f,
        [Description("HalfOp")]
        h,
        [Description("Invite Only")]
        i,
        [Description("Invite Exempt")]
        I,
        [Description("Join Throttling")]
        j,
        [Description("Key")]
        k,
        [Description("No KNOCK")]
        K,
        [Description("Limit")]
        l,
        [Description("Link")]
        L,
        [Description("Moderated")]
        m,
        [Description("Registered Nicks May Talk")]
        M,
        [Description("No External Messages")]
        n,
        [Description("No Nickname Changes")]
        N,
        [Description("Operator")]
        o,
        [Description("Oper Only")]
        O,
        [Description("Private")]
        p,
        [Description("Owner")]
        q,
        [Description("No Kicks Allowed")]
        Q,
        [Description("Registered")]
        r,
        [Description("Registered Only")]
        R,
        [Description("Secret")]
        s,
        [Description("Strip Mirc Colors")]
        S,
        [Description("+hoaq Only Topic Change")]
        t,
        [Description("No Notices")]
        T,
        [Description("Auditorium")]
        u,
        [Description("Voice")]
        v,
        [Description("No Invite")]
        V,
        [Description("Secure Only")]
        z
    }

    public enum UserMode
    {
        [Description("Service Administrator")]
        a,
        [Description("Service Administrator")]
        A,
        [Description("Bot")]
        B,
        [Description("Connection Notice")]
        c,
        [Description("C-oAdministrator")]
        C,
        [Description("Deaf")]
        d,
        [Description("View 'Eyes' Server Messages")]
        e,
        [Description("Flood Alerts")]
        f,
        [Description("Remote Server Connection Notice")]
        F,
        [Description("GlobOp and LocOp Messages")]
        g,
        [Description("Censored")]
        G,
        [Description("Helpful")]
        h,
        [Description("Hide IRCop Status")]
        H,
        [Description("Invisible")]
        i,
        [Description("Junk Messages")]
        j,
        [Description("Kill Messages")]
        k,
        [Description("Nick Change Notice")]
        n,
        [Description("Network Administrator")]
        N,
        [Description("Global Operator")]
        o,
        [Description("Local Operator")]
        O,
        [Description("Hide All Channels")]
        p,
        [Description("U:Lines Only")]
        q,
        [Description("Registered")]
        r,
        [Description("Registered Messages Only")]
        R,
        [Description("Server Notices")]
        s,
        [Description("Services Only")]
        S,
        [Description("VHost")]
        t,
        [Description("No CTCP")]
        T,
        [Description("Receive Bad DCC")]
        v,
        [Description("WebTV User")]
        V,
        [Description("Wallops")]
        w,
        [Description("Whois Notice")]
        W,
        [Description("Hidden Hostname")]
        x,
        [Description("Secure Connection")]
        z
    }

    public enum ServerStat
    {
        [Description("List of Servers that allow Server Connection")]
        c,
        [Description("List of Server Hubs")]
        h,
        [Description("List of Hosts that allow Client Connection")]
        i,
        [Description("List of banned user/hostname of Server")]
        k,
        [Description("Lists Server Connections")]
        l,
        [Description("Commands Supported")]
        m,
        [Description("Lists Hosts that allow Normal Operators")]
        o,
        [Description("List Class lines from Server Config")]
        y,
        [Description("Server Uptime")]
        u
    }

    public class BotError
    {
        public ErrorType Type { get; set; }
        public string Message { get; set; }
    }

    public class Nick
    {
        public string Realname { get; set; }
        public string Host { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }
        public bool Identified { get; set; }
        public bool Registered { get; set; }
        public List<UserMode> Modes { get; set; }

        public Nick()
        {
            Realname = string.Empty;
            Host = string.Empty;
            Nickname = string.Empty;
            Password = string.Empty;
            Identified = false;
            Registered = false;
            Modes = new List<UserMode>();
        }

        public Nick(string realname, string host, string nickname, string password, bool identified, bool registered, List<UserMode> modes)
        {
            Realname = realname;
            Host = host;
            Nickname = nickname;
            Password = password;
            Identified = identified;
            Registered = registered;
            Modes = modes;
        }

        public void AddMode(UserMode mode)
        {
            Modes.Add(mode);
        }

        public void AddModes(List<UserMode> modes)
        {
            Modes.AddRange(modes);
        }

        public void RemoveMode(UserMode mode)
        {
            if (Modes.Contains(mode))
            {
                Modes.Remove(mode);
            }
        }

        public void RemoveModes(List<UserMode> modes)
        {
            foreach (UserMode mode in modes)
            {
                RemoveMode(mode);
            }
        }
    }

    public class Channel
    {
        public string Name { get; set; }
        public string Topic { get; set; }
        public string Key { get; set; }
        public bool AutoJoin { get; set; }
        public DateTime Registration { get; set; }
        public List<ChannelMode> Modes { get; set; }
        public List<Nick> Nicks { get; set; }

        public Channel()
        {
            Name = string.Empty;
            Topic = string.Empty;
            Key = string.Empty;
            AutoJoin = false;
            Registration = DateTime.Now;
            Modes = new List<ChannelMode>();
            Nicks = new List<Nick>();
        }

        public Channel(string name, string topic, string key, bool autojoin, DateTime registration, List<ChannelMode> modes, List<Nick> nicks)
        {
            Name = name;
            Topic = topic;
            Key = key;
            AutoJoin = autojoin;
            Registration = registration;
            Modes = modes;
            Nicks = nicks;
        }

        public void AddNick(Nick nick)
        {
            Nicks.Add(nick);
        }

        public void AddNicks(List<Nick> nicks)
        {
            Nicks.AddRange(nicks);
        }

        public void RemoveNick(Nick nick)
        {
            if (Nicks.Contains(nick))
            {
                Nicks.Remove(nick);
            }
        }

        public void RemoveNicks(List<Nick> nicks)
        {
            foreach (Nick nick in nicks)
            {
                RemoveNick(nick);
            }
        }

        public void AddMode(ChannelMode mode)
        {
            Modes.Add(mode);
        }

        public void AddModes(List<ChannelMode> modes)
        {
            Modes.AddRange(modes);
        }

        public void RemoveMode(ChannelMode mode)
        {
            if (Modes.Contains(mode))
            {
                Modes.Remove(mode);
            }
        }

        public void RemoveModes(List<ChannelMode> modes)
        {
            foreach (ChannelMode mode in modes)
            {
                RemoveMode(mode);
            }
        }
    }

    public class Server
    {
        public string Name { get; set; }
        public List<IPEndPoint> Hosts { get; set; }
        public List<Channel> Channels { get; set; }
        public bool AutoConnect { get; set; }
        public Nick Nick { get; set; }
    }

    public class Message
    {
        public MessageType Type { get; set; }
        public Nick Sender { get; set; }
        public Nick Receiver { get; set; }
        public Channel Channel { get; set; }
        public string Parameters { get; set; }
    }

    public class Sender
    {

    }
}
