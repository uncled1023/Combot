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
                        IRC.IRCSendJoin(channel.Name, channel.Key);
                    }
                }
            }
        }

        private void HandleChannelMessageReceivedEvent(object sender, ChannelMessage e)
        {
            // The message was a command
            if (e.Message.StartsWith(ServerConfig.CommandPrefix))
            {
                string[] msgArgs = e.Message.Split(new char[] {' '}, 2, StringSplitOptions.RemoveEmptyEntries);
                string command = msgArgs[0].Remove(0, ServerConfig.CommandPrefix.Length);
                List<string> argsOnly = msgArgs.ToList();
                argsOnly.RemoveAt(0);
                if (_Modules.Exists(module => module.Commands.Exists(cmd => cmd.Triggers.Contains(command)) && module.Loaded))
                {
                    CommandMessage newCommand = new CommandMessage();
                    newCommand.Nick.Copy(e.Sender);
                    IRC.Channels.ForEach(channel => channel.Nicks.ForEach(nick =>
                    {
                        if (nick.Nickname == newCommand.Nick.Nickname)
                        {
                            newCommand.Nick.AddPrivileges(nick.Privileges);
                        }
                    }));
                    newCommand.TimeStamp = e.TimeStamp;
                    newCommand.ModuleName =_Modules.Find(module => module.Commands.Exists(cmd => cmd.Triggers.Contains(command)) && module.Loaded).Name;
                    newCommand.Command = command;
                    newCommand.Arguments.AddRange(argsOnly);

                    if (CommandReceivedEvent != null)
                    {
                        CommandReceivedEvent(newCommand);
                    }
                }
            }
        }
    }
}
