using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Combot.IRCServices
{
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
        [Description("Founder")]
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
        [Description("Co-Administrator")]
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

    public enum PrivilegeMode
    {
        [Description("Voice")]
        v,
        [Description("Half-Operator")]
        h,
        [Description("Operator")]
        o,
        [Description("Super Operator")]
        a,
        [Description("Founder")]
        q
    }
}
