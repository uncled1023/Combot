using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Combot.IRCServices.Messaging;
using Combot.IRCServices.TCP;

namespace Combot.IRCServices
{
    public partial class IRC
    {
        public List<Channel> Channels = new List<Channel>();
        public Messages Message;
        public event Action ConnectEvent;
        public event Action DisconnectEvent;
        public event Action<TCPError> TCPErrorEvent;
        public string Nickname { get; set; }
        public Dictionary<string, PrivilegeMode> PrivilegeMapping = new Dictionary<string, PrivilegeMode>() { { "+", PrivilegeMode.v }, { "%", PrivilegeMode.h }, { "@", PrivilegeMode.o }, { "&", PrivilegeMode.a }, { "~", PrivilegeMode.q } };

        private Thread TCPReader;
        private event Action<string> TCPMessageEvent;
        private readonly TCPInterface _TCP;
        private readonly ReaderWriterLockSlim ChannelRWLock;

        public IRC()
        {
            _TCP = new TCPInterface();
            Message = new Messages(this);
            Nickname = string.Empty;
            ChannelRWLock = new ReaderWriterLockSlim();

            TCPMessageEvent += Message.ParseTCPMessage;
            _TCP.TCPConnectionEvent += HandleTCPConnection;
            _TCP.TCPErrorEvent += HandleTCPError;
            Message.ErrorMessageEvent += HandleErrorMessage;
            Message.PingEvent += HandlePing;
            Message.ServerReplyEvent += HandleReply;
            Message.ChannelModeChangeEvent += HandleChannelModeChange;
            Message.UserModeChangeEvent += HandleUserModeChange;
            Message.NickChangeEvent += HandleNickChange;
            Message.JoinChannelEvent += HandleJoin;
            Message.PartChannelEvent += HandlePart;
            Message.KickEvent += HandleKick;
            Message.QuitEvent += HandleQuit;
        }

        /// <summary>
        /// Starts a TCP connection to the specified host.
        /// </summary>
        /// <param name="IP">The IP address of the host.</param>
        /// <param name="port">The port for the tcp connection.</param>
        /// <param name="readTimeout">The timeout for read operations in milliseconds.</param>
        /// <param name="allowedFailedCount">Number of times a read can fail before disconnecting.</param>
        /// <returns></returns>
        public bool Connect(IPAddress IP, int port, int readTimeout = 5000, int allowedFailedCount = 0)
        {
            bool result = false;
            if (!_TCP.Connected)
            {
                result = _TCP.Connect(IP, port, readTimeout, allowedFailedCount);
                if (result)
                {
                    TCPReader = new Thread(ReadTCPMessages);
                    TCPReader.IsBackground = true;
                    TCPReader.Start();

                    if (ConnectEvent != null)
                    {
                        ConnectEvent();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Disconencts from the active TCP connection.
        /// </summary>
        /// <returns></returns>
        public bool Disconnect()
        {
            bool result = false;

            if (_TCP.Connected)
            {
                _TCP.Disconnect();
            }

            ChannelRWLock.EnterWriteLock();
            Channels = new List<Channel>();
            ChannelRWLock.ExitWriteLock();

            if (DisconnectEvent != null)
            {
                DisconnectEvent();
            }

            return result;
        }

        /// <summary>
        /// Logs in the specified nick using their Username and Realname.
        /// </summary>
        /// <param name="serverName">The server's name.</param>
        /// <param name="nick">The nick information for the login.</param>
        public void Login(string serverName, Nick nick)
        {
            Nickname = nick.Nickname;
            SendNick(nick.Nickname);
            SendUser(nick.Username, nick.Host, serverName, nick.Realname);
        }

        private void ReadTCPMessages()
        {
            while (_TCP.Connected)
            {
                string response = ReadTCPMessage();
                if (TCPMessageEvent != null && response != null && response != string.Empty)
                {
                    TCPMessageEvent(response);
                }

                Thread.Sleep(10);
            }
        }

        private string ReadTCPMessage()
        {
            if (_TCP.Connected)
            {
                return _TCP.Read();
            }
            return null;
        }

        private void SendTCPMessage(string message)
        {
            if (_TCP.Connected)
            {
                _TCP.Write(message);
            }
        }

        private void HandleTCPConnection(int e)
        {
            if (DisconnectEvent != null)
            {
                DisconnectEvent();
            }
        }

        private void HandleTCPError(TCPError e)
        {
            if (TCPErrorEvent != null)
            {
                TCPErrorEvent(e);
            }
        }

        private void HandleErrorMessage(object sender, ErrorMessage e)
        {
            Disconnect();
        }

        /// <summary>
        /// Responds with PONG on a PING with the specified arguments.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandlePing(object sender, PingInfo e)
        {
            SendPong(e.Message);
        }

        private void HandleReply(object sender, IReply e)
        {
            if (e.GetType() == typeof(ServerReplyMessage))
            {
                ServerReplyMessage msg = (ServerReplyMessage)e;
                switch (msg.ReplyCode)
                {
                    // If we get a WHO response, we parse and add the nicks to the specified channel if they are not there already.
                    case IRCReplyCode.RPL_WHOREPLY:
                        ChannelRWLock.EnterWriteLock();
                        string[] msgSplit = msg.Message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        string target = msgSplit[0];
                        if (target.StartsWith("&") || target.StartsWith("#"))
                        {
                            if (msgSplit.GetUpperBound(0) >= 7)
                            {
                                string nickname = msgSplit[4];
                                string realname = msgSplit[7];
                                string username = msgSplit[1];
                                string host = msgSplit[2];
                                string modeString = msgSplit[5];
                                Channel channel = Channels.Find(chan => chan.Name == target);
                                if (channel != null)
                                {
                                    Nick nick = channel.GetNick(nickname);
                                    bool nickFound = true;
                                    if (nick == null)
                                    {
                                        nickFound = false;
                                        nick = new Nick();
                                    }
                                    nick.Nickname = nickname;
                                    nick.Host = host;
                                    nick.Realname = realname;
                                    nick.Username = username;
                                    nick.Modes = new List<UserMode>();
                                    nick.Privileges = new List<PrivilegeMode>();
                                    char[] modeArr = modeString.ToCharArray();
                                    for (int i = 1; i <= modeArr.GetUpperBound(0); i++)
                                    {
                                        if (PrivilegeMapping.ContainsKey(modeArr[i].ToString()))
                                        {
                                            nick.Privileges.Add(PrivilegeMapping[modeArr[i].ToString()]);
                                        }
                                        else if (modeArr[i].ToString() == "*")
                                        {
                                            nick.Modes.Add(UserMode.o);
                                        }
                                        else
                                        {
                                            nick.Modes.Add((UserMode)Enum.Parse(typeof(UserMode), modeArr[i].ToString()));
                                        }
                                    }
                                    if (!nickFound)
                                    {
                                        channel.AddNick(nick);
                                    }
                                }
                            }
                        }
                        ChannelRWLock.ExitWriteLock();
                        break;
                    // On a topic reply, update the channel's topic
                    case IRCReplyCode.RPL_TOPIC:
                        ChannelRWLock.EnterWriteLock();
                        string[] topicSplit = msg.Message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        string topicChan = topicSplit[0];
                        Channel topicChannel = Channels.Find(chan => chan.Name == topicChan);
                        if (topicChannel != null)
                        {
                            topicChannel.Topic = topicSplit[1].Remove(0, 1);
                        }
                        ChannelRWLock.ExitWriteLock();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                ServerErrorMessage msg = (ServerErrorMessage)e;
            }
        }

        /// <summary>
        /// Update a channel's mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleChannelModeChange(object sender, ChannelModeChangeInfo e)
        {
            ChannelRWLock.EnterWriteLock();
            Channel channel = Channels.Find(chan => chan.Name == e.Channel);
            if (channel != null)
            {
                foreach (ChannelModeInfo mode in e.Modes)
                {
                    switch (mode.Mode)
                    {
                        case ChannelMode.v:
                        case ChannelMode.h:
                        case ChannelMode.o:
                        case ChannelMode.a:
                        case ChannelMode.q:
                            Nick changedNick = channel.GetNick(mode.Parameter);
                            if (mode.Set)
                            {
                                changedNick.AddPrivilege((PrivilegeMode)Enum.Parse(typeof(PrivilegeMode), mode.Mode.ToString()));
                            }
                            else
                            {
                                changedNick.RemovePrivilege((PrivilegeMode)Enum.Parse(typeof(PrivilegeMode), mode.Mode.ToString()));
                            }
                            break;
                        case ChannelMode.b:
                            if (mode.Set)
                            {
                                channel.AddBan(mode.Parameter);
                            }
                            else
                            {
                                channel.RemoveBan(mode.Parameter);
                            }
                            break;
                        case ChannelMode.k:
                            if (mode.Set)
                            {
                                channel.AddMode(mode.Mode);
                                channel.Key = mode.Parameter;
                            }
                            else
                            {
                                channel.RemoveMode(mode.Mode);
                                channel.Key = string.Empty;
                            }
                            break;
                        default:
                            if (mode.Set)
                            {
                                channel.AddMode(mode.Mode);
                            }
                            else
                            {
                                channel.RemoveMode(mode.Mode);
                            }
                            break;
                    }
                }
            }
            ChannelRWLock.ExitWriteLock();
        }

        /// <summary>
        /// Update a nick's mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleUserModeChange(object sender, UserModeChangeInfo e)
        {
            ChannelRWLock.EnterWriteLock();
            for (int i = 0; i < Channels.Count; i++)
            {
                Nick changedNick = Channels[i].GetNick(e.Nick.Nickname);
                if (changedNick != null)
                {
                    foreach (UserModeInfo mode in e.Modes)
                    {
                        if (mode.Set)
                        {
                            changedNick.AddMode(mode.Mode);
                        }
                        else
                        {
                            changedNick.RemoveMode(mode.Mode);
                        }
                    }
                }
            }
            ChannelRWLock.ExitWriteLock();
        }

        /// <summary>
        /// Update a nick to use their new nickname.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleNickChange(object sender, NickChangeInfo e)
        {
            ChannelRWLock.EnterWriteLock();
            for (int i = 0; i < Channels.Count; i++)
            {
                Nick newNick = Channels[i].GetNick(e.OldNick.Nickname);
                if (newNick != null)
                {
                    newNick.Nickname = e.NewNick.Nickname;
                }
            }
            ChannelRWLock.ExitWriteLock();
        }

        /// <summary>
        /// Add a nick to a channel on join.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleJoin(object sender, JoinChannelInfo e)
        {
            ChannelRWLock.EnterWriteLock();
            Channel channel = Channels.Find(chan => chan.Name == e.Channel);
            if (channel != null)
            {
                channel.AddNick(e.Nick);
            }
            else
            {
                Channel newChannel = new Channel();
                newChannel.Name = e.Channel;
                if (e.Nick.Nickname == Nickname)
                {
                    newChannel.Joined = true;
                }
                newChannel.Nicks.Add(e.Nick);
                Channels.Add(newChannel);
                SendWho(newChannel.Name);
            }
            ChannelRWLock.ExitWriteLock();
        }

        /// <summary>
        /// Remove a nick from a channel on part.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandlePart(object sender, PartChannelInfo e)
        {
            ChannelRWLock.EnterWriteLock();
            Channel channel = Channels.Find(chan => chan.Name == e.Channel);
            if (channel != null)
            {
                channel.RemoveNick(e.Nick.Nickname);
            }
            ChannelRWLock.ExitWriteLock();
        }

        /// <summary>
        /// Remove a nick from a channel on kick.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKick(object sender, KickInfo e)
        {
            ChannelRWLock.EnterWriteLock();
            Channel channel = Channels.Find(chan => chan.Name == e.Channel);
            if (channel != null)
            {
                channel.RemoveNick(e.Nick.Nickname);
            }
            ChannelRWLock.ExitWriteLock();
        }

        /// <summary>
        /// Remove a nick from all channels on quit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleQuit(object sender, QuitInfo e)
        {
            ChannelRWLock.EnterWriteLock();
            for (int i = 0; i < Channels.Count; i++)
            {
                Channels[i].RemoveNick(e.Nick.Nickname);
            }
            ChannelRWLock.ExitWriteLock();
        }
    }
}
