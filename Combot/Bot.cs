using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Combot.IRCServices;
using Combot.Configurations;
using Combot.IRCServices.Messaging;
using Combot.Modules;
using Module = Combot.Modules.Module;

namespace Combot
{
    public class Bot
    {
        public event Action<CommandMessage> CommandReceivedEvent;
        public event Action<BotError> ErrorEvent;
        public ServerConfig ServerConfig;
        public IRC IRC;
        public bool Connected = false;
        public static Dictionary<PrivilegeMode, AccessType> AccessTypeMapping = new Dictionary<PrivilegeMode, AccessType>() { { PrivilegeMode.v, AccessType.Voice }, { PrivilegeMode.h, AccessType.HalfOperator }, { PrivilegeMode.o, AccessType.Operator }, { PrivilegeMode.a, AccessType.SuperOperator }, { PrivilegeMode.q, AccessType.Founder } };

        private List<Module> _Modules;

        public Bot(ServerConfig serverConfig)
        {
            IRC = new IRC();
            _Modules = new List<Module>();
            ServerConfig = serverConfig;
            IRC.ConnectEvent += HandleConnectEvent;
            IRC.DisconnectEvent += HandleDisconnectEvent;
            IRC.Message.ServerReplyEvent += HandleReplyEvent;
            IRC.Message.ChannelMessageReceivedEvent += HandleChannelMessageReceivedEvent;

            LoadModules();
        }

        /// <summary>
        /// Trys to connect to one of the IPs of the given hostname.  If the connection was successful, it will login the nick.
        /// </summary>
        public void Connect()
        {
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
                IRC.Login(ServerConfig.Name, new Nick() { Nickname = ServerConfig.Nickname, Host = Dns.GetHostName(), Realname = ServerConfig.Realname, Username = ServerConfig.Username });
            }
        }

        /// <summary>
        /// Disconnects from the current server.
        /// </summary>
        public void Disconnect()
        {
            IRC.Disconnect();
            Connected = false;
        }

        public void LoadModules()
        {
            foreach (Module module in ServerConfig.Modules)
            {
                if (module.Enabled && !_Modules.Exists(mod => mod.ClassName == module.ClassName))
                {
                    Module loadedModule = module.CreateInstance(this);
                    if (loadedModule.Loaded)
                    {
                        _Modules.Add(loadedModule);
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

        private void HandleReplyEvent(object sender, IReply e)
        {
            if (e.GetType() == typeof(ServerReplyMessage))
            {
                ServerReplyMessage reply = (ServerReplyMessage)e;
                // If the reply is Welcome, that means we are fully connected to the server and can now join the auto-join channels.
                if (reply.ReplyCode == IRCReplyCode.RPL_WELCOME && Connected)
                {
                    foreach (ChannelConfig channel in ServerConfig.Channels)
                    {
                        IRC.SendJoin(channel.Name, channel.Key);
                    }
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

            Module module = _Modules.Find(mod => mod.Commands.Exists(c => c.Triggers.Contains(command)) && mod.Loaded);
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
                        string[] argSplit = argsOnly.First()
                            .Split(new[] {' '}, cmd.Arguments.Count + 1, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < cmd.Arguments.Count; i++)
                        {
                            newCommand.Arguments.Add(argSplit[i]);
                        }
                    }

                    if (CommandReceivedEvent != null)
                    {
                        CommandReceivedEvent(newCommand);
                    }
                }
            }
        }
    }
}
