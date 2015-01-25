using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Combot.IRCServices.Messaging
{
    public class Messages
    {
        public event EventHandler<IReply> ServerReplyEvent;
        public event EventHandler<ErrorMessage> ErrorMessageEvent;
        public event EventHandler<ChannelMessage> ChannelMessageReceivedEvent;
        public event EventHandler<PrivateMessage> PrivateMessageReceivedEvent;
        public event EventHandler<ServerNotice> ServerNoticeReceivedEvent;
        public event EventHandler<ChannelNotice> ChannelNoticeReceivedEvent;
        public event EventHandler<PrivateNotice> PrivateNoticeReceivedEvent;
        public event EventHandler<TopicChangeInfo> TopicChangeEvent;
        public event EventHandler<ChannelModeChangeInfo> ChannelModeChangeEvent;
        public event EventHandler<UserModeChangeInfo> UserModeChangeEvent;
        public event EventHandler<JoinChannelInfo> JoinChannelEvent;
        public event EventHandler<PartChannelInfo> PartChannelEvent;
        public event EventHandler<KickInfo> KickEvent;
        public event EventHandler<QuitInfo> QuitEvent;
        public event EventHandler<PingInfo> PingEvent;
        public event EventHandler<PongInfo> PongEvent;

        private IRC _IRC;

        internal Messages(IRC irc)
        {
            _IRC = irc;
        }

        internal void ParseTCPMessage(string tcpMessage)
        {
            DateTime messageTime = DateTime.Now;
            Regex messageRegex = new Regex(@"^:(?<Sender>[^\s]+)\s(?<Type>[^\s]+)\s(?<Recipient>[^\s]+)\s(?<Args>.*)", RegexOptions.None);
            Regex senderRegex = new Regex(@"^:(?<Nick>[^\s]+)!(?<Realname>[^\s]+)@(?<Host>[^\s]+)", RegexOptions.None);
            Regex pingRegex = new Regex(@"^PING :(?<Message>.+)", RegexOptions.None);
            Regex pongRegex = new Regex(@"^PONG :(?<Message>.+)", RegexOptions.None);
            Regex errorRegex = new Regex(@"^ERROR :(?<Message>.+)", RegexOptions.None);

            string[] messages = tcpMessage.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string message in messages)
            {
                if (messageRegex.IsMatch(message))
                {
                    Match match = messageRegex.Match(message);
                    string type = match.Groups["Type"].Value;
                    string sender = match.Groups["Sender"].Value;
                    string recipient = match.Groups["Recipient"].Value;
                    string args = match.Groups["Args"].Value;
                    Match senderMatch = senderRegex.Match(sender);
                    string senderNick = sender;
                    string senderRealname = sender;
                    string senderHost = sender;
                    if (senderMatch.Success)
                    {
                        senderNick = senderMatch.Groups["Nick"].Value;
                        senderRealname = senderMatch.Groups["Realname"].Value;
                        senderHost = senderMatch.Groups["Nick"].Value;
                    }

                    int replyCode;
                    if (int.TryParse(type, out replyCode))
                    {
                        // The message was a reply to a command sent
                        if (Enum.IsDefined(typeof(IRCReplyCode), replyCode))
                        {
                            if (ServerReplyEvent != null)
                            {
                                ServerReplyEvent(this, new ServerReplyMessage() { TimeStamp = messageTime, ReplyCode = (IRCReplyCode)replyCode, Message = args.Remove(0, 1) });
                            }
                        }
                        else if (Enum.IsDefined(typeof(IRCErrorCode), replyCode))
                        {
                            if (ServerReplyEvent != null)
                            {
                                ServerReplyEvent(this, new ServerErrorMessage() { TimeStamp = messageTime, ErrorCode = (IRCErrorCode)replyCode, Message = args.Remove(0, 1) });
                            }
                        }
                    }
                    else
                    {
                        switch (type)
                        {
                            case "PRIVMSG":
                                if (recipient.StartsWith("&") || recipient.StartsWith("#"))
                                {
                                    ChannelMessage msg = new ChannelMessage();
                                    msg.Channel = _IRC.Channels.Find(channel => channel.Name == recipient);
                                    if (msg.Channel != null && msg.Channel.Modes != null && msg.Channel.Modes.Contains(ChannelMode.n))
                                    {
                                        msg.Sender = msg.Channel.Nicks.Find(nick => nick.Nickname == senderNick);
                                    }
                                    else
                                    {
                                        msg.Sender = new Nick() { Nickname = senderNick, Realname = senderRealname, Host = senderHost };
                                    }
                                    msg.Message = args.Remove(0, 1);

                                    if (ChannelMessageReceivedEvent != null)
                                    {
                                        ChannelMessageReceivedEvent(this, msg);
                                    }
                                }
                                else
                                {
                                    PrivateMessage msg = new PrivateMessage();
                                    msg.Sender = new Nick() { Nickname = senderNick, Realname = senderRealname, Host = senderHost };
                                    msg.Message = args.Remove(0, 1);

                                    if (PrivateMessageReceivedEvent != null)
                                    {
                                        PrivateMessageReceivedEvent(this, msg);
                                    }
                                }
                                break;
                            case "NOTICE":
                                if (recipient.StartsWith("&") || recipient.StartsWith("#"))
                                {
                                    ChannelNotice msg = new ChannelNotice();
                                    msg.Channel = _IRC.Channels.Find(channel => channel.Name == recipient);
                                    if (msg.Channel != null && msg.Channel.Modes != null && msg.Channel.Modes.Contains(ChannelMode.n))
                                    {
                                        msg.Sender = msg.Channel.Nicks.Find(nick => nick.Nickname == senderNick);
                                    }
                                    else
                                    {
                                        msg.Sender = new Nick() { Nickname = senderNick, Realname = senderRealname, Host = senderHost };
                                    }
                                    msg.Message = args.Remove(0, 1);

                                    if (ChannelNoticeReceivedEvent != null)
                                    {
                                        ChannelNoticeReceivedEvent(this, msg);
                                    }
                                }
                                else
                                {
                                    PrivateNotice msg = new PrivateNotice();
                                    msg.Sender = new Nick() { Nickname = senderNick, Realname = senderRealname, Host = senderHost };
                                    msg.Message = args.Remove(0, 1);

                                    if (PrivateNoticeReceivedEvent != null)
                                    {
                                        PrivateNoticeReceivedEvent(this, msg);
                                    }
                                }
                                break;
                            case "MODE":
                                if (recipient.StartsWith("&") || recipient.StartsWith("#"))
                                {
                                    ChannelModeChangeInfo modeMsg = new ChannelModeChangeInfo();
                                    modeMsg.Modes = new List<ChannelModeInfo>();
                                    modeMsg.Channel = _IRC.Channels.Find(channel => channel.Name == recipient);
                                    if (modeMsg.Channel != null && modeMsg.Channel.Nicks != null && modeMsg.Channel.Nicks.Exists(nick => nick.Nickname == senderNick))
                                    {
                                        modeMsg.Nick = modeMsg.Channel.Nicks.Find(nick => nick.Nickname == senderNick);
                                    }
                                    else
                                    {
                                        modeMsg.Nick = new Nick() { Nickname = senderNick, Realname = senderRealname, Host = senderHost };
                                    }

                                    string[] modeArgs = args.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    char[] modeInfo = modeArgs[0].TrimStart(':').ToCharArray();
                                    bool set = true;
                                    int argIndex = 1;
                                    foreach (char mode in modeInfo)
                                    {
                                        if (mode.Equals('-'))
                                        {
                                            set = false;
                                        }
                                        else if (mode.Equals('+'))
                                        {
                                            set = true;
                                        }
                                        else
                                        {
                                            ChannelModeInfo newMode = new ChannelModeInfo();
                                            newMode.Set = set;
                                            newMode.Mode = (ChannelMode)Enum.Parse(typeof(ChannelMode), mode.ToString());
                                            if (modeArgs.GetUpperBound(0) > argIndex)
                                            {
                                                switch (newMode.Mode)
                                                {
                                                    case ChannelMode.k:
                                                    case ChannelMode.l:
                                                    case ChannelMode.v:
                                                    case ChannelMode.h:
                                                    case ChannelMode.o:
                                                    case ChannelMode.a:
                                                    case ChannelMode.q:
                                                    case ChannelMode.b:
                                                    case ChannelMode.e:
                                                    case ChannelMode.I:
                                                        newMode.Parameter = modeArgs[argIndex];
                                                        argIndex++;
                                                        break;
                                                    default:
                                                        newMode.Parameter = string.Empty;
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                newMode.Parameter = string.Empty;
                                            }
                                            modeMsg.Modes.Add(newMode);
                                        }
                                    }

                                    if (ChannelModeChangeEvent != null)
                                    {
                                        ChannelModeChangeEvent(this, modeMsg);
                                    }
                                }
                                else
                                {
                                    UserModeChangeInfo modeMsg = new UserModeChangeInfo();
                                    modeMsg.Modes = new List<UserModeInfo>();

                                    string[] modeArgs = args.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    char[] modeInfo = modeArgs[0].TrimStart(':').ToCharArray();
                                    bool set = true;
                                    foreach (char mode in modeInfo)
                                    {
                                        if (mode.Equals('-'))
                                        {
                                            set = false;
                                        }
                                        else if (mode.Equals('+'))
                                        {
                                            set = true;
                                        }
                                        else
                                        {
                                            UserModeInfo newMode = new UserModeInfo();
                                            newMode.Set = set;
                                            newMode.Mode = (UserMode)Enum.Parse(typeof(UserMode), mode.ToString());
                                            modeMsg.Modes.Add(newMode);
                                        }
                                    }

                                    if (UserModeChangeEvent != null)
                                    {
                                        UserModeChangeEvent(this, modeMsg);
                                    }
                                }
                                break;
                            case "TOPIC":
                                TopicChangeInfo topicMsg = new TopicChangeInfo();
                                topicMsg.Channel = _IRC.Channels.Find(channel => channel.Name == recipient);
                                if (topicMsg.Channel != null && topicMsg.Channel.Nicks != null && topicMsg.Channel.Nicks.Exists(nick => nick.Nickname == senderNick))
                                {
                                    topicMsg.Nick = topicMsg.Channel.Nicks.Find(nick => nick.Nickname == senderNick);
                                }
                                else
                                {
                                    topicMsg.Nick = new Nick() { Nickname = senderNick, Realname = senderRealname, Host = senderHost };
                                }
                                topicMsg.Topic = args.Remove(0, 1);

                                if (TopicChangeEvent != null)
                                {
                                    TopicChangeEvent(this, topicMsg);
                                }
                                break;
                            case "JOIN":
                                JoinChannelInfo joinMsg = new JoinChannelInfo();
                                joinMsg.Channel = _IRC.Channels.Find(channel => channel.Name == recipient.TrimStart(':'));
                                joinMsg.Nick = new Nick() { Nickname = senderNick, Realname = senderRealname, Host = senderHost };

                                if (JoinChannelEvent != null)
                                {
                                    JoinChannelEvent(this, joinMsg);
                                }
                                break;
                            case "PART":
                                PartChannelInfo partMsg = new PartChannelInfo();
                                partMsg.Channel = _IRC.Channels.Find(channel => channel.Name == recipient);
                                partMsg.Nick = new Nick() { Nickname = senderNick, Realname = senderRealname, Host = senderHost };

                                if (PartChannelEvent != null)
                                {
                                    PartChannelEvent(this, partMsg);
                                }
                                break;
                            case "KICK":
                                KickInfo kickMsg = new KickInfo();
                                kickMsg.Channel = _IRC.Channels.Find(channel => channel.Name == recipient);
                                kickMsg.Nick = new Nick() { Nickname = senderNick, Realname = senderRealname, Host = senderHost };
                                string[] argSplit = args.Split(new char[] { ' ' }, StringSplitOptions.None);
                                kickMsg.KickedNick = kickMsg.Channel.Nicks.Find(nick => nick.Nickname == argSplit[0]);
                                List<string> reasonArgs = argSplit.ToList<string>();
                                reasonArgs.RemoveAt(0);
                                kickMsg.Reason = string.Join(" ", reasonArgs.ToArray()).Remove(0, 1);

                                if (KickEvent != null)
                                {
                                    KickEvent(this, kickMsg);
                                }
                                break;
                            case "QUIT":
                                QuitInfo quitMsg = new QuitInfo();
                                quitMsg.Nick = new Nick() { Nickname = senderNick, Realname = senderRealname, Host = senderHost };
                                quitMsg.Message = recipient.Remove(0, 1);

                                if (QuitEvent != null)
                                {
                                    QuitEvent(this, quitMsg);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                else if (pingRegex.IsMatch(message))
                {
                    Match match = pingRegex.Match(message);
                    PingInfo ping = new PingInfo();
                    ping.Message = match.Groups["Message"].Value;

                    if (PingEvent != null)
                    {
                        PingEvent(this, ping);
                    }
                }
                else if (pongRegex.IsMatch(message))
                {
                    Match match = pongRegex.Match(message);
                    PongInfo pong = new PongInfo();
                    pong.Message = match.Groups["Message"].Value;

                    if (PongEvent != null)
                    {
                        PongEvent(this, pong);
                    }
                }
                else if (errorRegex.IsMatch(message))
                {
                    Match match = errorRegex.Match(message);
                    ErrorMessage error = new ErrorMessage();
                    error.Message = match.Groups["Message"].Value;

                    if (ErrorMessageEvent != null)
                    {
                        ErrorMessageEvent(this, error);
                    }
                }
            }
        }

        internal bool GetReply(List<IRCReplyCode> ReplyCodes, List<IRCErrorCode> ErrorCodes)
        {
            GetReply reply = new GetReply();
            reply.Replies = ReplyCodes;
            reply.Errors = ErrorCodes;
            ServerReplyEvent += (sender, e) => HandleReply(sender, e, reply);
            reply.Ready.Wait(TimeSpan.FromMilliseconds(5000));
            reply.Reattach = false;
            return reply.Result;
        }

        private void HandleReply(object sender, IReply message, GetReply reply)
        {
            bool replyFound = false;
            if (message.GetType() == typeof(ServerReplyMessage))
            {
                ServerReplyMessage msg = (ServerReplyMessage)message;
                replyFound = reply.Replies.Contains(msg.ReplyCode);
            }
            else
            {
                ServerErrorMessage msg = (ServerErrorMessage)message;
                replyFound = reply.Errors.Contains(msg.ErrorCode);
            }
            if (replyFound)
            {
                reply.Result = replyFound;
                reply.Ready.Set();
            }
            else
            {
                if (reply.Reattach)
                {
                    ServerReplyEvent += (obj, e) => HandleReply(obj, e, reply);
                }
            }
        }
    }
}
