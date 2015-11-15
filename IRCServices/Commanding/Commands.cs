using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Combot.IRCServices.Messaging;

namespace Combot.IRCServices.Commanding
{
    public class Commands
    {
        public event EventHandler<PrivateMessageCommand> PrivateMessageCommandEvent;
        public event EventHandler<PrivateNoticeCommand> PrivateNoticeCommandEvent;
        public event EventHandler<CTCPMessageCommand> CTCPMessageCommandEvent;
        public event EventHandler<CTCPNoticeCommand> CTCPNoticeCommandEvent;
        public event EventHandler<PasswordCommand> PasswordCommandEvent;
        public event EventHandler<NickCommand> NickCommandEvent;
        public event EventHandler<UserCommand> UserCommandEvent;
        public event EventHandler<OperCommand> OperCommandEvent;
        public event EventHandler<QuitCommand> QuitCommandEvent;
        public event EventHandler<JoinCommand> JoinCommandEvent;
        public event EventHandler<PartCommand> PartCommandEvent;
        public event EventHandler<ChannelModeCommand> ChannelModeCommandEvent;
        public event EventHandler<UserModeCommand> UserModeCommandEvent;
        public event EventHandler<TopicCommand> TopicCommandEvent;
        public event EventHandler<NamesCommand> NamesCommandEvent;
        public event EventHandler<ListCommand> ListCommandEvent;
        public event EventHandler<InviteCommand> InviteCommandEvent;
        public event EventHandler<KickCommand> KickCommandEvent;
        public event EventHandler<VersionCommand> VersionCommandEvent;
        public event EventHandler<StatsCommand> StatsCommandEvent;
        public event EventHandler<LinksCommand> LinksCommandEvent;
        public event EventHandler<TimeCommand> TimeCommandEvent;
        public event EventHandler<ConnectCommand> ConnectCommandEvent;
        public event EventHandler<TraceCommand> TraceCommandEvent; 
        public event EventHandler<AdminCommand> AdminCommandEvent;
        public event EventHandler<InfoCommand> InfoCommandEvent;
        public event EventHandler<WhoCommand> WhoCommandEvent;
        public event EventHandler<WhoisCommand> WhoisCommandEvent;
        public event EventHandler<WhowasCommand> WhowasCommandEvent;
        public event EventHandler<KillCommand> KillCommandEvent;
        public event EventHandler<PingCommand> PingCommandEvent;
        public event EventHandler<PongCommand> PongCommandEvent;
        public event EventHandler<AwayCommand> AwayCommandEvent;
        public event EventHandler<RehashCommand> RehashCommandEvent;
        public event EventHandler<RestartCommand> RestartCommandEvent; 
        public event EventHandler<SummonCommand> SummonCommandEvent;
        public event EventHandler<UsersCommand> UsersCommandEvent;
        public event EventHandler<WallopsCommand> WallopsCommandEvent;
        public event EventHandler<UserhostCommand> UserhostCommandEvent;
        public event EventHandler<IsonCommand> IsonCommandEvent; 

        private IRC _IRC;
        private int MaxMessageLength;

        public Commands(IRC irc, int maxMessageLength, int messageSendDelay)
        {
            _IRC = irc;
            MaxMessageLength = maxMessageLength;
        }

        /// <summary>
        /// Sends a private message to a nick or channel
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="message"></param>
        public void SendPrivateMessage(string recipient, string message)
        {
            if (message.Length > MaxMessageLength)
            {
                List<string> splitMessage = message.Split(new char[] { ' ' }).ToList();
                string subMessage = string.Empty;
                string nextMessage = string.Empty;
                for (int i = 0; i < splitMessage.Count; i++)
                {
                    int wordLength = splitMessage[i].Length + 1;
                    int totalLength = subMessage.Length;
                    int leftover = MaxMessageLength - totalLength;
                    if (totalLength + wordLength > MaxMessageLength)
                    {
                        if (wordLength > MaxMessageLength)
                        {
                            if (leftover > 0)
                            {
                                string firstPart = splitMessage[i].Substring(0, leftover);
                                subMessage = string.Join(" ", subMessage, firstPart);
                            }
                            string lastPart = (leftover > 0) ? splitMessage[i].Substring(leftover, (wordLength - leftover) - 1) : splitMessage[i];
                            nextMessage = string.Join(" ", new string[]
                            {
                                lastPart,
                                (splitMessage.Count > i + 1) ? string.Join(" ", splitMessage.GetRange(i + 1, splitMessage.Count - i)) : string.Empty
                            
                            });
                        }
                        else
                        {
                            nextMessage = string.Join(" ", splitMessage.GetRange(i, splitMessage.Count - i));
                        }
                        break;
                    }
                    subMessage = string.Join(" ", subMessage, splitMessage[i]);
                }
                if (subMessage != string.Empty)
                {
                    _IRC.SendTCPMessage(string.Format("PRIVMSG {0} :{1}", recipient, subMessage.Remove(0, 1)));
                    if (PrivateMessageCommandEvent != null)
                    {
                        PrivateMessageCommandEvent(this, new PrivateMessageCommand {Message = subMessage.Remove(0, 1), Recipient = recipient});
                    }
                }
                if (nextMessage != string.Empty)
                {
                    SendPrivateMessage(recipient, nextMessage);
                }
            }
            else
            {
                _IRC.SendTCPMessage(string.Format("PRIVMSG {0} :{1}", recipient, message));
                if (PrivateMessageCommandEvent != null)
                {
                    PrivateMessageCommandEvent(this, new PrivateMessageCommand { Message = message, Recipient = recipient });
                }
            }
        }

        public void SendPrivateMessage(List<string> recipients, string message)
        {
            string recipient_list = string.Empty;
            foreach (string recipient in recipients)
            {
                recipient_list += recipient + ",";
            }
            SendPrivateMessage(recipient_list.TrimEnd(','), message);
        }

        /// <summary>
        /// Sends a Notice to either a nick or channel
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="message"></param>
        public void SendNotice(string recipient, string message)
        {
            if (message.Length > MaxMessageLength)
            {
                List<string> splitMessage = message.Split(new char[] { ' ' }).ToList();
                string subMessage = string.Empty;
                string nextMessage = string.Empty;
                for (int i = 0; i < splitMessage.Count; i++)
                {
                    int wordLength = splitMessage[i].Length + 1;
                    int totalLength = subMessage.Length;
                    int leftover = MaxMessageLength - totalLength;
                    if (totalLength + wordLength > MaxMessageLength)
                    {
                        if (wordLength > MaxMessageLength)
                        {
                            if (leftover > 0)
                            {
                                string firstPart = splitMessage[i].Substring(0, leftover);
                                subMessage = string.Join(" ", subMessage, firstPart);
                            }
                            string lastPart = (leftover > 0) ? splitMessage[i].Substring(leftover, (wordLength - leftover) - 1) : splitMessage[i];
                            nextMessage = string.Join(" ", new string[]
                            {
                                lastPart,
                                (splitMessage.Count > i + 1) ? string.Join(" ", splitMessage.GetRange(i + 1, splitMessage.Count - i)) : string.Empty

                            });
                        }
                        else
                        {
                            nextMessage = string.Join(" ", splitMessage.GetRange(i, splitMessage.Count - i));
                        }
                        break;
                    }
                    subMessage = string.Join(" ", subMessage, splitMessage[i]);
                }
                if (subMessage != string.Empty)
                {
                    _IRC.SendTCPMessage(string.Format("NOTICE {0} :{1}", recipient, subMessage.Remove(0, 1)));
                    if (PrivateNoticeCommandEvent != null)
                    {
                        PrivateNoticeCommandEvent(this, new PrivateNoticeCommand {Message = subMessage.Remove(0, 1), Recipient = recipient});
                    }
                }
                if (nextMessage != string.Empty)
                {
                    SendNotice(recipient, nextMessage);
                }
            }
            else
            {
                _IRC.SendTCPMessage(string.Format("NOTICE {0} :{1}", recipient, message));
                if (PrivateNoticeCommandEvent != null)
                {
                    PrivateNoticeCommandEvent(this, new PrivateNoticeCommand { Message = message, Recipient = recipient });
                }
            }
        }

        public void SendNotice(List<string> recipients, string message)
        {
            string recipient_list = string.Empty;
            foreach (string recipient in recipients)
            {
                recipient_list += recipient + ",";
            }

            SendNotice(recipient_list.TrimEnd(','), message);
        }

        /// <summary>
        /// Sends a CTCP command and optional message to a nick or channel
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="command"></param>
        /// <param name="message"></param>
        public void SendCTCPMessage(string recipient, string command, string message = "")
        {
            if (message != string.Empty)
            {
                message = " " + message;
            }
            _IRC.SendTCPMessage(string.Format("PRIVMSG {0} :\u0001{1}{2}\u0001", recipient, command, message));
            if (CTCPMessageCommandEvent != null)
            {
                CTCPMessageCommandEvent(this, new CTCPMessageCommand { Arguments = message, Command = command, Recipient = recipient });
            }
        }

        public void SendCTCPMessage(List<string> recipients, string command, string message)
        {
            string recipient_list = string.Empty;
            foreach (string recipient in recipients)
            {
                recipient_list += recipient + ",";
            }
            if (message != string.Empty)
            {
                message = " " + message;
            }
            SendCTCPMessage(recipient_list.TrimEnd(','), command, message);
        }

        /// <summary>
        /// Sends a CTCP command and optional message to a nick or channel
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="command"></param>
        /// <param name="message"></param>
        public void SendCTCPNotice(string recipient, string command, string message = "")
        {
            if (message != string.Empty)
            {
                message = " " + message;
            }
            _IRC.SendTCPMessage(string.Format("NOTICE {0} :\u0001{1}{2}\u0001", recipient, command, message));
            if (CTCPNoticeCommandEvent != null)
            {
                CTCPNoticeCommandEvent(this, new CTCPNoticeCommand { Arguments = message, Command = command, Recipient = recipient });
            }
        }

        public void SendCTCPNotice(List<string> recipients, string command, string message)
        {
            string recipient_list = string.Empty;
            foreach (string recipient in recipients)
            {
                recipient_list += recipient + ",";
            }
            if (message != string.Empty)
            {
                message = " " + message;
            }
            SendCTCPNotice(recipient_list.TrimEnd(','), command, message);
        }

        /// <summary>
        /// Sends the connection password
        /// </summary>
        /// <param name="password"></param>
        public void SendPassword(string password)
        {
            _IRC.SendTCPMessage(string.Format("PASSWORD {0}", password));
            if (PasswordCommandEvent != null)
            {
                PasswordCommandEvent(this, new PasswordCommand { Password = password });
            }
        }

        /// <summary>
        /// Sends a Nick command to set the nickname
        /// </summary>
        /// <param name="nick"></param>
        public void SendNick(string nick)
        {
            _IRC.SendTCPMessage(string.Format("NICK {0}", nick));
            if (NickCommandEvent != null)
            {
                NickCommandEvent(this, new NickCommand { Nick = nick });
            }
        }

        /// <summary>
        /// Sends the User command to set a user
        /// </summary>
        /// <param name="user"></param>
        public void SendUser(string username, string hostname, string servername, string realname)
        {
            _IRC.SendTCPMessage(string.Format("USER {0} {1} {2} :{3}", username, hostname, servername, realname));
            if (UserCommandEvent != null)
            {
                UserCommandEvent(this, new UserCommand { Username = username, Hostname = hostname, Servername = servername, Realname = realname });
            }
        }

        /// <summary>
        /// Sends the Oper command to authorize the client as a newtork Oper
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void SendOper(string username, string password)
        {
            _IRC.SendTCPMessage(string.Format("OPER {0} {1}", username, password));
            if (OperCommandEvent != null)
            {
                OperCommandEvent(this, new OperCommand {Username = username, Password = password});
            }
        }

        /// <summary>
        /// Sends a Quit command to end the client session
        /// </summary>
        /// <param name="message"></param>
        public void SendQuit()
        {
            _IRC.SendTCPMessage("QUIT");
            if (QuitCommandEvent != null)
            {
                QuitCommandEvent(this, new QuitCommand());
            }
        }

        public void SendQuit(string message)
        {
            _IRC.SendTCPMessage(string.Format("QUIT :{0}", message));
            if (QuitCommandEvent != null)
            {
                QuitCommandEvent(this, new QuitCommand {Message = message});
            }
        }

        /// <summary>
        /// Sends a Join command to join a channel
        /// </summary>
        /// <param name="channel"></param>
        public void SendJoin(string channel, string key = "")
        {
            string message = string.Empty;
            message = (key != string.Empty) ? string.Format("{0} {1}", channel, key) : channel;
            _IRC.SendTCPMessage(string.Format("JOIN {0}", message));
            if (JoinCommandEvent != null)
            {
                JoinCommandEvent(this, new JoinCommand {Channel = channel, Key = key});
            }
        }

        public void SendJoin(List<string> channels, List<string> keys)
        {
            string message = string.Empty;
            string channel_string = string.Empty;
            string key_string = string.Empty;

            foreach (string channel in channels)
            {
                channel_string += channel + ",";
            }
            foreach (string key in keys)
            {
                if (key != string.Empty)
                {
                    key_string += key + ",";
                }
            }
            channel_string = channel_string.TrimEnd(',');
            key_string = key_string.TrimEnd(',');

            message = (key_string != string.Empty) ? string.Format("{0} {1}", channel_string, key_string) : channel_string;
            _IRC.SendTCPMessage(string.Format("JOIN {0}", message));
            if (JoinCommandEvent != null)
            {
                JoinCommandEvent(this, new JoinCommand {Channel = channel_string, Key = key_string});
            }
        }

        /// <summary>
        /// Sends a Part command to leave a channel
        /// </summary>
        /// <param name="channel"></param>
        public void SendPart(string channel)
        {
            _IRC.SendTCPMessage(string.Format("PART {0}", channel));
            if (PartCommandEvent != null)
            {
                PartCommandEvent(this, new PartCommand {Channel = channel});
            }
        }

        public void SendPart(List<string> channels)
        {
            string channel_list = string.Empty;
            foreach (string channel in channels)
            {
                channel_list += channel + ",";
            }

            SendPart(channel_list.TrimEnd(','));
        }


        /// <summary>
        /// Sends a Mode command for either a channel mode or user mode
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="mode"></param>
        public void SendMode(string channel, ChannelModeInfo modeInfo)
        {
            string mode_set = modeInfo.Set ? "+" : "-";
            _IRC.SendTCPMessage(string.Format("MODE {0} {1} {2}", channel, mode_set + modeInfo.Mode, modeInfo.Parameter));
            if (ChannelModeCommandEvent != null)
            {
                ChannelModeCommandEvent(this, new ChannelModeCommand {Channel = channel, Mode = modeInfo});
            }
        }

        public void SendMode(string channel, List<ChannelModeInfo> modeInfos)
        {
            string modeList = string.Empty;
            string setList = string.Empty;
            bool lastSet = true;
            int modeIndex = 1;
            foreach (ChannelModeInfo modeInfo in modeInfos)
            {
                if ((setList.Length + modeList.Length + channel.Length + modeInfo.Parameter.Length + 8 + ((modeInfo.Set != lastSet) ? 1 : 0)) > MaxMessageLength || modeIndex > 4)
                {
                    _IRC.SendTCPMessage(string.Format("MODE {0} {1} {2}", channel, setList, modeList));
                    setList = string.Empty;
                    modeList = string.Empty;
                    lastSet = true;
                    modeIndex = 1;
                }
                if (modeInfo.Set != lastSet)
                    setList += modeInfo.Set ? "+" : "-";

                setList += modeInfo.Mode;
                modeList += modeInfo.Parameter + " ";

                lastSet = modeInfo.Set;
                modeIndex++;
                if (ChannelModeCommandEvent != null)
                {
                    ChannelModeCommandEvent(this, new ChannelModeCommand { Channel = channel, Mode = modeInfo });
                }
            }
            if (!string.IsNullOrEmpty(setList) && !string.IsNullOrEmpty(modeList))
                _IRC.SendTCPMessage(string.Format("MODE {0} {1} {2}", channel, setList, modeList));
        }

        public void SendMode(string nick, UserModeInfo modeInfo)
        {
            string mode_set = modeInfo.Set ? "+" : "-";
            _IRC.SendTCPMessage(string.Format("MODE {0} {1}", nick, mode_set + modeInfo.Mode));
            if (UserModeCommandEvent != null)
            {
                UserModeCommandEvent(this, new UserModeCommand {Nick = nick, Mode = modeInfo});
            }
        }

        public void SendMode(string nick, List<UserModeInfo> modeInfos)
        {
            foreach (UserModeInfo modeInfo in modeInfos)
            {
                SendMode(nick, modeInfo);
            }
        }

        /// <summary>
        /// Sends a Topic command to change the channels topic or view the current one
        /// </summary>
        /// <param name="channel"></param>
        public void SendTopic(string channel)
        {
            _IRC.SendTCPMessage(string.Format("TOPIC {0}", channel));
            if (TopicCommandEvent != null)
            {
                TopicCommandEvent(this, new TopicCommand {Channel = channel});
            }
        }

        public void SendTopic(string channel, string topic)
        {
            _IRC.SendTCPMessage(string.Format("TOPIC {0} :{1}", channel, topic));
            if (TopicCommandEvent != null)
            {
                TopicCommandEvent(this, new TopicCommand {Channel = channel, Topic = topic});
            }
        }

        /// <summary>
        /// Sends a Names command to get a list of visible users
        /// </summary>
        public void SendNames()
        {
            _IRC.SendTCPMessage("NAMES");
            if (NamesCommandEvent != null)
            {
                NamesCommandEvent(this, new NamesCommand());
            }
        }

        public void SendNames(string channel)
        {
            _IRC.SendTCPMessage(string.Format("NAMES {0}", channel));
            if (NamesCommandEvent != null)
            {
                NamesCommandEvent(this, new NamesCommand {Channel = channel});
            }
        }

        public void SendNames(List<string> channels)
        {
            string channel_list = string.Empty;
            foreach (string channel in channels)
            {
                channel_list += channel + ",";
            }
            SendNames(channel_list.TrimEnd(','));
        }

        /// <summary>
        /// Sends a List command to get the topic of channels
        /// </summary>
        public void SendList()
        {
            _IRC.SendTCPMessage("LIST");
            if (ListCommandEvent != null)
            {
                ListCommandEvent(this, new ListCommand());
            }
        }

        public void SendList(string channel)
        {
            _IRC.SendTCPMessage(string.Format("LIST {0}", channel));
            if (ListCommandEvent != null)
            {
                ListCommandEvent(this, new ListCommand {Channel = channel});
            }
        }

        public void SendList(List<string> channels)
        {
            string channel_list = string.Empty;
            foreach (string channel in channels)
            {
                channel_list += channel + ",";
            }
            SendList(channel_list.TrimEnd(','));
        }

        /// <summary>
        /// Sends an Invite command that invites the specified nick to the channel 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="nick"></param>
        public void SendInvite(string channel, string nick)
        {
            _IRC.SendTCPMessage(string.Format("INVITE {0} {1}", nick, channel));
            if (InviteCommandEvent != null)
            {
                InviteCommandEvent(this, new InviteCommand {Channel = channel, Nick = nick});
            }
        }

        /// <summary>
        /// Sends a Kick command to remove a user from a channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="nick"></param>
        public void SendKick(string channel, string nick)
        {
            _IRC.SendTCPMessage(string.Format("KICK {0} {1}", channel, nick));
            if (KickCommandEvent != null)
            {
                KickCommandEvent(this, new KickCommand {Channel = channel, Nick = nick});
            }
        }

        public void SendKick(string channel, string nick, string reason)
        {
            _IRC.SendTCPMessage(string.Format("KICK {0} {1} :{2}", channel, nick, reason));
            if (KickCommandEvent != null)
            {
                KickCommandEvent(this, new KickCommand {Channel = channel, Nick = nick, Reason = reason});
            }
        }

        /// <summary>
        /// Sends a Version command to the server to get a Version reply
        /// </summary>
        /// <param name="server"></param>
        public void SendVersion(string server)
        {
            _IRC.SendTCPMessage(string.Format("VERSION {0}", server));
            if (VersionCommandEvent != null)
            {
                VersionCommandEvent(this, new VersionCommand {Server = server});
            }
        }

        /// <summary>
        /// Sends a Stats command to view Server information and statistics
        /// </summary>
        /// <param name="stat"></param>
        public void SendStats(ServerStat stat)
        {
            _IRC.SendTCPMessage(string.Format("STATS {0}", stat));
            if (StatsCommandEvent != null)
            {
                StatsCommandEvent(this, new StatsCommand {Stat = stat.ToString()});
            }
        }

        public void SendStats(ServerStat stat, string parameter)
        {
            _IRC.SendTCPMessage(string.Format("STATS {0} {1}", stat, parameter));
            if (StatsCommandEvent != null)
            {
                StatsCommandEvent(this, new StatsCommand {Stat = stat.ToString(), Parameter = parameter});
            }
        }

        /// <summary>
        /// Sends a Links command to list all servers matching a mask
        /// </summary>
        /// <param name="mask"></param>
        public void SendLinks(string mask)
        {
            _IRC.SendTCPMessage(string.Format("LINKS {0}", mask));
            if (LinksCommandEvent != null)
            {
                LinksCommandEvent(this, new LinksCommand {Mask = mask});
            }
        }

        public void SendLinks(string server, string mask)
        {
            _IRC.SendTCPMessage(string.Format("LINKS {0} {1}", mask, server));
            if (LinksCommandEvent != null)
            {
                LinksCommandEvent(this, new LinksCommand {Mask = mask, Server = server});
            }
        }

        /// <summary>
        /// Sends a Time command to query the local server time
        /// </summary>
        public void SendTime()
        {
            _IRC.SendTCPMessage("TIME");
            if (TimeCommandEvent != null)
            {
                TimeCommandEvent(this, new TimeCommand());
            }
        }

        public void SendTime(string server)
        {
            _IRC.SendTCPMessage(string.Format("TIME {0}", server));
            if (TimeCommandEvent != null)
            {
                TimeCommandEvent(this, new TimeCommand {Server = server});
            }
        }

        /// <summary>
        /// Senda a Connect command to have the server try to connect to another server
        /// </summary>
        /// <param name="server"></param>
        public void SendConnect(string server)
        {
            _IRC.SendTCPMessage(string.Format("CONNECT {0}", server));
            if (ConnectCommandEvent != null)
            {
                ConnectCommandEvent(this, new ConnectCommand {Server = server});
            }
        }

        public void SendConnect(string server, string originator, int port)
        {
            _IRC.SendTCPMessage(string.Format("CONNECT {0} {1} {2}", originator, port, server));
            if (ConnectCommandEvent != null)
            {
                ConnectCommandEvent(this, new ConnectCommand {Server = server, Originator = originator, Port = port});
            }
        }

        /// <summary>
        /// Sends a Trace command to find the route to the target (nick or server)
        /// </summary>
        /// <param name="target"></param>
        public void SendTrace(string target)
        {
            _IRC.SendTCPMessage(string.Format("TRACE {0}", target));
            if (TraceCommandEvent != null)
            {
                TraceCommandEvent(this, new TraceCommand {Target = target});
            }
        }

        /// <summary>
        /// Sends an Admin command to get the name of the server Administrator
        /// </summary>
        public void SendAdmin()
        {
            _IRC.SendTCPMessage("ADMIN");
            if (AdminCommandEvent != null)
            {
                AdminCommandEvent(this, new AdminCommand());
            }
        }

        public void SendAdmin(string host)
        {
            _IRC.SendTCPMessage(string.Format("ADMIN {0}", host));
            if (AdminCommandEvent != null)
            {
                AdminCommandEvent(this, new AdminCommand {Host = host});
            }
        }

        /// <summary>
        /// Sends an Info command for a specific server or nick
        /// </summary>
        /// <param name="host"></param>
        public void SendInfo(string host)
        {
            _IRC.SendTCPMessage(string.Format("INFO {0}", host));
            if (InfoCommandEvent != null)
            {
                InfoCommandEvent(this, new InfoCommand {Host = host});
            }
        }

        /// <summary>
        /// Sends a Who command to list all public users or matching a mask
        /// </summary>
        public void SendWho()
        {
            _IRC.SendTCPMessage("WHO");
            if (WhoCommandEvent != null)
            {
                WhoCommandEvent(this, new WhoCommand());
            }
        }

        public void SendWho(string host, bool ops = false)
        {
            string msg = string.Empty;
            if (ops)
            {
                msg = string.Format("WHO {0} o", host);
            }
            else
            {
                msg = string.Format("WHO {0}", host);
            }
            _IRC.SendTCPMessage(msg);
            if (WhoCommandEvent != null)
            {
                WhoCommandEvent(this, new WhoCommand {Host = host});
            }
        }

        /// <summary>
        /// Sends a Whois command to get info about a user
        /// </summary>
        /// <param name="nick"></param>
        public void SendWhois(string nick)
        {
            _IRC.SendTCPMessage(string.Format("WHOIS {0}", nick));
            if (WhoisCommandEvent != null)
            {
                WhoisCommandEvent(this, new WhoisCommand {Nick = nick});
            }
        }

        public void SendWhois(string nick, string server)
        {
            _IRC.SendTCPMessage(string.Format("WHOIS {0} {1}", server, nick));
            if (WhoisCommandEvent != null)
            {
                WhoisCommandEvent(this, new WhoisCommand {Nick = nick, Server = server});
            }
        }

        /// <summary>
        /// Sends a Whowas command to get the nick history of a user
        /// </summary>
        /// <param name="nick"></param>
        public void SendWhowas(string nick)
        {
            _IRC.SendTCPMessage(string.Format("WHOWAS {0}", nick));
            if (WhowasCommandEvent != null)
            {
                WhowasCommandEvent(this, new WhowasCommand {Nick = nick});
            }
        }

        public void SendWhowas(string nick, int entries)
        {
            _IRC.SendTCPMessage(string.Format("WHOWAS {0} {1}", nick, entries));
            if (WhowasCommandEvent != null)
            {
                WhowasCommandEvent(this, new WhowasCommand {Nick = nick, Entries = entries});
            }
        }

        public void SendWhowas(string nick, int entries, string server)
        {
            _IRC.SendTCPMessage(string.Format("WHOWAS {0} {1} {2}", nick, entries, server));
            if (WhowasCommandEvent != null)
            {
                WhowasCommandEvent(this, new WhowasCommand {Nick = nick, Entries = entries, Server = server});
            }
        }

        /// <summary>
        /// Sends a Kill command to disconnect a nick
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="comment"></param>
        public void SendKill(string nick, string comment)
        {
            _IRC.SendTCPMessage(string.Format("KILL {0} {1}", nick, comment));
            if (KillCommandEvent != null)
            {
                KillCommandEvent(this, new KillCommand {Nick = nick, Comment = comment});
            }
        }

        /// <summary>
        /// Sends a Ping command to the recipient
        /// </summary>
        /// <param name="recipient"></param>
        public void SendPing(string recipient)
        {
            _IRC.SendTCPMessage(string.Format("PING {0}", recipient));
            if (PingCommandEvent != null)
            {
                PingCommandEvent(this, new PingCommand {Recipient = recipient});
            }
        }

        /// <summary>
        /// Sends a PONG response to respond to a Ping
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="recipient"></param>
        public void SendPong()
        {
            _IRC.SendTCPMessage("PONG");
            if (PongCommandEvent != null)
            {
                PongCommandEvent(this, new PongCommand());
            }
        }

        public void SendPong(string message)
        {
            _IRC.SendTCPMessage(string.Format("PONG {0}", message));
            if (PongCommandEvent != null)
            {
                PongCommandEvent(this, new PongCommand());
            }
        }

        public void SendPong(string sender, string recipient)
        {
            _IRC.SendTCPMessage(string.Format("PONG {0} {1}", sender, recipient));
            if (PongCommandEvent != null)
            {
                PongCommandEvent(this, new PongCommand { Sender = sender, Recipient = recipient });
            }
        }

        /// <summary>
        /// Sends an Away command to unset away status
        /// </summary>
        public void SendAway()
        {
            _IRC.SendTCPMessage("AWAY");
            if (AwayCommandEvent != null)
            {
                AwayCommandEvent(this, new AwayCommand());
            }
        }

        /// <summary>
        /// Sends an Away comand to set away status with auto-reply message
        /// </summary>
        /// <param name="message"></param>
        public void SendAway(string message)
        {
            _IRC.SendTCPMessage(string.Format("AWAY {0}", message));
            if (AwayCommandEvent != null)
            {
                AwayCommandEvent(this, new AwayCommand {Message = message});
            }
        }

        /// <summary>
        /// Sends a Rehash command to the server to reload it's configuration file
        /// </summary>
        public void SendRehash()
        {
            _IRC.SendTCPMessage("REHASH");
            if (RehashCommandEvent != null)
            {
                RehashCommandEvent(this, new RehashCommand());
            }
        }

        /// <summary>
        /// Sends a Restart command to the server to restart
        /// </summary>
        public void SendRestart()
        {
            _IRC.SendTCPMessage("RESTART");
            if (RestartCommandEvent != null)
            {
                RestartCommandEvent(this, new RestartCommand());
            }
        }

        /// <summary>
        /// Sends a Summon command to summon a nick to the server
        /// </summary>
        /// <param name="nick"></param>
        public void SendSummon()
        {
            _IRC.SendTCPMessage("SUMMON");
            if (SummonCommandEvent != null)
            {
                SummonCommandEvent(this, new SummonCommand());
            }
        }

        public void SendSummon(string nick)
        {
            _IRC.SendTCPMessage(string.Format("SUMMON {0}", nick));
            if (SummonCommandEvent != null)
            {
                SummonCommandEvent(this, new SummonCommand {Nick = nick});
            }
        }

        public void SendSummon(string nick, string host)
        {
            _IRC.SendTCPMessage(string.Format("SUMMON {0} {1}", nick, host));
            if (SummonCommandEvent != null)
            {
                SummonCommandEvent(this, new SummonCommand {Nick = nick, Host = host});
            }
        }

        /// <summary>
        /// Sends a Users command to get a list of Users from a server
        /// </summary>
        /// <param name="server"></param>
        public void SendUsers(string server)
        {
            _IRC.SendTCPMessage(string.Format("USERS {0}", server));
            if (UsersCommandEvent != null)
            {
                UsersCommandEvent(this, new UsersCommand {Server = server});
            }
        }

        /// <summary>
        /// Sends a Wallops command which sends a message to all connected ops
        /// </summary>
        /// <param name="message"></param>
        public void SendWallops(string message)
        {
            _IRC.SendTCPMessage(string.Format("WALLOPS :{0}", message));
            if (WallopsCommandEvent != null)
            {
                WallopsCommandEvent(this, new WallopsCommand {Message = message});
            }
        }

        /// <summary>
        /// Sends an Userhost command to up to 5 nicknames to return information about each nick
        /// </summary>
        /// <param name="nicks"></param>
        public void SendUserhost(List<string> nicks)
        {
            string message = string.Empty;
            foreach (string nick in nicks)
            {
                message += " " + nick;
            }
            _IRC.SendTCPMessage(string.Format("USERHOST {0}", message.Trim()));
            if (UserhostCommandEvent != null)
            {
                UserhostCommandEvent(this, new UserhostCommand {Nicks = message.Trim()});
            }
        }

        /// <summary>
        /// Sends an IsOn command to get a return if the nicks specified are online
        /// </summary>
        /// <param name="nicks"></param>
        public void SendIson(List<string> nicks)
        {
            string message = string.Empty;
            foreach (string nick in nicks)
            {
                message += " " + nick;
            }
            _IRC.SendTCPMessage(string.Format("ISON {0}", message.Trim()));
            if (IsonCommandEvent != null)
            {
                IsonCommandEvent(this, new IsonCommand {Nicks = message.Trim()});
            }
        }
    }
}