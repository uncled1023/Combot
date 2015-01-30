using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Combot.IRCServices;
using Combot.Configurations;
using Combot.IRCServices.Messaging;
using Combot.Modules;

namespace Combot
{
    public class Bot
    {
        public event Action<CommandMessage> CommandReceivedEvent;
        public event Action<BotError> ErrorEvent;
        public ServerConfig ServerConfig;
        public IRC IRC;
        public List<Module> Modules;
        public bool Connected = false;
        public bool LoggedIn = false;
        public static Dictionary<PrivilegeMode, AccessType> AccessTypeMapping = new Dictionary<PrivilegeMode, AccessType>() { { PrivilegeMode.v, AccessType.Voice }, { PrivilegeMode.h, AccessType.HalfOperator }, { PrivilegeMode.o, AccessType.Operator }, { PrivilegeMode.a, AccessType.SuperOperator }, { PrivilegeMode.q, AccessType.Founder } };

        private bool GhostSent;

        public Bot(ServerConfig serverConfig)
        {
            IRC = new IRC();
            Modules = new List<Module>();
            GhostSent = false;
            ServerConfig = serverConfig;
            IRC.ConnectEvent += HandleConnectEvent;
            IRC.DisconnectEvent += HandleDisconnectEvent;
            IRC.Message.ServerReplyEvent += HandleReplyEvent;
            IRC.Message.ChannelMessageReceivedEvent += HandleChannelMessageReceivedEvent;
            IRC.Message.JoinChannelEvent += HandleJoinEvent;
            IRC.Message.KickEvent += HandleKickEvent;

            LoadModules();
        }

        /// <summary>
        /// Trys to connect to one of the IPs of the given hostname.  If the connection was successful, it will login the nick.
        /// </summary>
        public void Connect()
        {
            GhostSent = false;
            bool serverConnected = false;
            int i = 0;
            do
            {
                if (ServerConfig.Hosts.Count > i)
                {
                    IPAddress[] ipList = Dns.GetHostAddresses(ServerConfig.Hosts[i].Host);
                    foreach (IPAddress ip in ipList)
                    {
                        serverConnected = IRC.Connect(ip, ServerConfig.Hosts[i].Port, 5000);
                        if (serverConnected)
                        {
                            break;
                        }
                    }
                    i++;
                }
                else
                {
                    break;
                }
            }
            while (!serverConnected);

            if (serverConnected)
            {
                IRC.Login(ServerConfig.Name, new Nick()
                {
                    Nickname = ServerConfig.Nickname, 
                    Host = Dns.GetHostName(), 
                    Realname = ServerConfig.Realname, 
                    Username = ServerConfig.Username
                });
            }
        }

        /// <summary>
        /// Disconnects from the current server.
        /// </summary>
        public void Disconnect()
        {
            IRC.Disconnect();
            Connected = false;
            LoggedIn = false;
        }

        public void LoadModules()
        {
            foreach (Module module in ServerConfig.Modules)
            {
                if (module.Enabled && !Modules.Exists(mod => mod.ClassName == module.ClassName))
                {
                    Module loadedModule = module.CreateInstance(this);
                    if (loadedModule.Loaded)
                    {
                        Modules.Add(loadedModule);
                    }
                }
            }
        }

        private void HandleConnectEvent()
        {
            Connected = true;
        }

        private void HandleDisconnectEvent()
        {
            Connected = false;
        }

        private void HandleJoinEvent(object sender, JoinChannelInfo info)
        {
            if (info.Nick.Nickname == IRC.Nickname)
            {
                if (!ServerConfig.Channels.Exists(chan => chan.Name == info.Channel))
                {
                    ChannelConfig chanConfig = new ChannelConfig();
                    chanConfig.Name = info.Channel;
                    ServerConfig.Channels.Add(chanConfig);
                    ServerConfig.Save();
                }
            }
        }

        private void HandleKickEvent(object sender, KickInfo info)
        {
            if (info.KickedNick.Nickname == IRC.Nickname)
            {
                ServerConfig.Channels.RemoveAll(chan => chan.Name == info.Channel);
                ServerConfig.Save();
            }
        }

        private async void HandleReplyEvent(object sender, IReply e)
        {
            if (e.GetType() == typeof(ServerReplyMessage))
            {
                ServerReplyMessage reply = (ServerReplyMessage)e;
                switch (reply.ReplyCode)
                {
                    case IRCReplyCode.RPL_WELCOME:
                        // If the reply is Welcome, that means we are fully connected to the server.
                        LoggedIn = true;
                        if (!GhostSent && IRC.Nickname != ServerConfig.Nickname)
                        {
                            IRC.SendPrivateMessage("NickServ", string.Format("GHOST {0} {1}", ServerConfig.Nickname, ServerConfig.Password));
                            Thread.Sleep(1000);
                            IRC.SendNick(ServerConfig.Nickname);
                            GhostSent = true;
                        }
                        // Identify to NickServ if need be
                        IRC.SendPrivateMessage("NickServ", string.Format("IDENTIFY {0}", ServerConfig.Password));

                        // Join all required channels
                        // Delay joining channels for configured time
                        Thread.Sleep(ServerConfig.JoinDelay);
                        foreach (ChannelConfig channel in ServerConfig.Channels)
                        {
                            IRC.SendJoin(channel.Name, channel.Key);
                        }
                        break;
                }
            }
            else if (e.GetType() == typeof(ServerErrorMessage))
            {
                ServerErrorMessage error = (ServerErrorMessage) e;
                switch (error.ErrorCode)
                {
                    case IRCErrorCode.ERR_NOTREGISTERED:
                        if (ServerConfig.Password != string.Empty && ServerConfig.Email != string.Empty)
                        {
                            IRC.SendPrivateMessage("NickServ", string.Format("REGISTER {0} {1}", ServerConfig.Password, ServerConfig.Email));
                        }
                        break;
                    case IRCErrorCode.ERR_NICKNAMEINUSE:
                        if (LoggedIn == false)
                        {
                            string nick = string.Empty;
                            if (IRC.Nickname == ServerConfig.Nickname && ServerConfig.SecondaryNickname != string.Empty)
                            {
                                nick = ServerConfig.SecondaryNickname;
                            }
                            else
                            {
                                Random rand = new Random();
                                nick = string.Format("{0}_{1}", ServerConfig.Nickname, rand.Next(100000).ToString());
                            }
                            IRC.Login(ServerConfig.Name, new Nick()
                            {
                                Nickname = nick,
                                Host = Dns.GetHostName(),
                                Realname = ServerConfig.Realname,
                                Username = ServerConfig.Username
                            });
                        }
                        break;
                }
            }
        }

        private void HandleChannelMessageReceivedEvent(object sender, ChannelMessage e)
        {
            // The message was a command
            if (e.Message.StartsWith(ServerConfig.CommandPrefix))
            {
                if (!ServerConfig.ChannelBlacklist.Contains(e.Channel)
                    && !ServerConfig.NickBlacklist.Contains(e.Sender.Nickname)
                    )
                {
                    ParseCommandMessage(e.TimeStamp, e.Message, e.Sender, e.Channel, LocationType.Channel);
                }
            }
        }

        private void ParseCommandMessage(DateTime timestamp, string message, Nick sender, string location, LocationType locationType)
        {
            // Extract command and arguments
            string[] msgArgs = message.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            string command = msgArgs[0].Remove(0, ServerConfig.CommandPrefix.Length);
            List<string> argsOnly = msgArgs.ToList();
            argsOnly.RemoveAt(0);

            Module module = Modules.Find(mod => mod.Commands.Exists(c => c.Triggers.Contains(command)) && mod.Loaded);
            if (module != null)
            {
                Command cmd = module.Commands.Find(c => c.Triggers.Contains(command));
                if (cmd != null)
                {
                    CommandMessage newCommand = new CommandMessage();
                    newCommand.Nick.Copy(sender);
                    IRC.Channels.ForEach(channel => channel.Nicks.ForEach(nick =>
                    {
                        if (nick.Nickname == newCommand.Nick.Nickname)
                        {
                            newCommand.Nick.AddPrivileges(nick.Privileges);
                        }
                    }));
                    newCommand.TimeStamp = timestamp;
                    newCommand.Location = location;
                    newCommand.LocationType = locationType;
                    newCommand.Command = command;
                    if (argsOnly.Count > 0)
                    {
                        string[] argSplit = argsOnly.First().Split(new[] {' '}, cmd.Arguments.Count + 1, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < cmd.Arguments.Count && i <= argSplit.GetUpperBound(0); i++)
                        {
                            newCommand.Arguments.Add(cmd.Arguments[i].Name, argSplit[i]);
                        }
                    }
                    if (cmd.Arguments.FindAll(arg => arg.Required).Count <= newCommand.Arguments.Count)
                    {
                        if (CommandReceivedEvent != null)
                        {
                            CommandReceivedEvent(newCommand);
                        }
                    }
                }
            }
        }
    }
}
