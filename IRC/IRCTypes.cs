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
        public Server Server { get; set; }
        public Channel Channel { get; set; }
        public Nick Sender { get; set; }
        public Nick Recipient { get; set; }
        public string Parameters { get; set; }
    }

    // IRC Reply Codes //
    public enum IRCReplyCode
    {
        RPL_WELCOME = 1,
        RPL_YOURHOST = 2,
        RPL_CREATED = 3,
        RPL_MYINFO = 4,
        RPL_BOUNCE = 5,
        RPL_TRACELINK = 200,
        RPL_TRACECONNECTING = 201,
        RPL_TRACEHANDSHAKE = 202,
        RPL_TRACEUNKNOWN = 203,
        RPL_TRACEOPERATOR = 204,
        RPL_TRACEUSER = 205,
        RPL_TRACESERVER = 206,
        RPL_TRACESERVICE = 207,
        RPL_TRACENEWTYPE = 208,
        RPL_TRACECLASS = 209,
        RPL_TRACERECONNECT = 210,
        RPL_STATSLINKINFO = 211,
        RPL_STATSCOMMANDS = 212,
        RPL_ENDOFSTATS = 219,
        RPL_UMODEIS = 221,
        RPL_SERVLIST = 234,
        RPL_SERVLISTEND = 235,
        RPL_STATSUPTIME = 242,
        RPL_STATSOLINE = 243,
        RPL_LUSERCLIENT = 251,
        RPL_LUSEROP = 252,
        RPL_LUSERUNKNOWN = 253,
        RPL_LUSERCHANNELS = 254,
        RPL_LUSERME = 255,
        RPL_ADMINME = 256,
        RPL_ADMINLOC1 = 257,
        RPL_ADMINLOC2 = 258,
        RPL_ADMINEMAIL = 259,
        RPL_TRACELOG = 261,
        RPL_TRACEEND = 262,
        RPL_TRYAGAIN = 263,
        RPL_AWAY = 301,
        RPL_USERHOST = 302,
        RPL_ISON = 303,
        RPL_UNAWAY = 305,
        RPL_NOWAWAY = 306,
        RPL_WHOISUSER = 311,
        RPL_WHOISSERVER = 312,
        RPL_WHOISOPERATOR = 313,
        RPL_WHOWASUSER = 314,
        RPL_ENDOFWHO = 315,
        RPL_WHOISIDLE = 317,
        RPL_ENDOFWHOIS = 318,
        RPL_WHOISCHANNELS = 319,
        RPL_LISTSTART = 321,
        RPL_LIST = 322,
        RPL_LISTEND = 323,
        RPL_CHANNELMODEIS = 324,
        RPL_UNIQOPIS = 325,
        RPL_NOTOPIC = 331,
        RPL_TOPIC = 332,
        RPL_INVITING = 341,
        RPL_SUMMONING = 342,
        RPL_INVITELIST = 346,
        RPL_ENDOFINVITELIST = 347,
        RPL_EXCEPTLIST = 348,
        RPL_ENDOFEXCEPTLIST = 349,
        RPL_VERSION = 351,
        RPL_WHOREPLY = 352,
        RPL_NAMREPLY = 353,
        RPL_LINKS = 364,
        RPL_ENDOFLINKS = 365,
        RPL_ENDOFNAMES = 366,
        RPL_BANLIST = 367,
        RPL_ENDOFBANLIST = 368,
        RPL_ENDOFWHOWAS = 369,
        RPL_INFO = 371,
        RPL_MOTD = 372,
        RPL_ENDOFINFO = 374,
        RPL_MOTDSTART = 375,
        RPL_ENDOFMOTD = 376,
        RPL_YOUREOPER = 381,
        RPL_REHASHING = 382,
        RPL_YOURESERVICE = 383,
        RPL_TIME = 391,
        RPL_USERSSTART = 392,
        RPL_USERS = 393,
        RPL_ENDOFUSERS = 394,
        RPL_NOUSERS = 395
    }

    // IRC Error Codes //
    public enum IRCErrorCode
    {
        ERR_NOSUCHNICK = 401,
        ERR_NOSUCHSERVER = 402,
        ERR_NOSUCHCHANNEL = 403,
        ERR_CANNOTSENDTOCHAN = 404,
        ERR_TOOMANYCHANNELS = 405,
        ERR_WASNOSUCHNICK = 406,
        ERR_TOOMANYTARGETS = 407,
        ERR_NOSUCHSERVICE = 408,
        ERR_NOORIGIN = 409,
        ERR_NORECIPIENT = 411,
        ERR_NOTEXTTOSEND = 412,
        ERR_NOTOPLEVEL = 413,
        ERR_WILDTOPLEVEL = 414,
        ERR_BADMASK = 415,
        ERR_UNKNOWNCOMMAND = 421,
        ERR_NOMOTD = 422,
        ERR_NOADMININFO = 423,
        ERR_FILEERROR = 424,
        ERR_NONICKNAMEGIVEN = 431,
        ERR_ERRONEUSNICKNAME = 432,
        ERR_NICKNAMEINUSE = 433,
        ERR_NICKCOLLISION = 436,
        ERR_UNAVAILRESOURCE = 437,
        ERR_USERNOTINCHANNEL = 441,
        ERR_NOTONCHANNEL = 442,
        ERR_USERONCHANNEL = 443,
        ERR_NOLOGIN = 444,
        ERR_SUMMONDISABLED = 445,
        ERR_USERSDISABLED = 446,
        ERR_NOTREGISTERED = 451,
        ERR_NEEDMOREPARAMS = 461,
        ERR_ALREADYREGISTRED = 462,
        ERR_NOPERMFORHOST = 463,
        ERR_PASSWDMISMATCH = 464,
        ERR_YOUREBANNEDCREEP = 465,
        ERR_YOUWILLBEBANNED = 466,
        ERR_KEYSET = 467,
        ERR_CHANNELISFULL = 471,
        ERR_UNKNOWNMODE = 472,
        ERR_INVITEONLYCHAN = 473,
        ERR_BANNEDFROMCHAN = 474,
        ERR_BADCHANNELKEY = 475,
        ERR_BADCHANMASK = 476,
        ERR_NOCHANMODES = 477,
        ERR_BANLISTFULL = 478,
        ERR_NOPRIVILEGES = 481,
        ERR_CHANOPRIVSNEEDED = 482,
        ERR_CANTKILLSERVER = 483,
        ERR_RESTRICTED = 484,
        ERR_UNIQOPPRIVSNEEDED = 485,
        ERR_NOOPERHOST = 491,
        ERR_UMODEUNKNOWNFLAG = 501,
        ERR_USERSDONTMATCH = 502
    }
}
