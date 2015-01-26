using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Combot.IRCServices;
using Combot.Configurations;
using Combot.IRCServices.Messaging;

namespace Combot
{
    public class Bot
    {
        public event Action<BotError> ErrorEvent;
        public ServerConfig ServerConfig;
        public IRC IRC;
        public bool Connected = false;

        public Bot(ServerConfig serverConfig)
        {
            IRC = new IRC();
            ServerConfig = serverConfig;
            IRC.ConnectEvent += HandleConnectEvent;
            IRC.DisconnectEvent += HandleDisconnectEvent;
            IRC.Message.ServerReplyEvent += HandleReplyEvent;
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
    }
}
