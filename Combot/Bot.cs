using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Combot.IRCServices;

namespace Combot
{
    public class Bot
    {
        public event Action<BotError> ErrorEvent;
        public Config Config;
        public IRC IRC;
        public bool Connected = false;

        public Bot()
        {
            Config = new Config();
            IRC = new IRC();

            IRC.DisconnectEvent += HandleDisconnectEvent;
        }

        public bool Connect()
        {
            int i = 0;
            do
            {
                if (Config.Server.Hosts.Count > i)
                {
                    Connected = IRC.Connect(Config.Server.Hosts[i].Address, Config.Server.Hosts[i].Port, 5000);
                    i++;
                }
                else
                {
                    break;
                }
            }
            while (!Connected);

            if (Connected)
            {
                IRC.Login(Config.Server.Name, new Nick() { Nickname = Config.Nick, Host = Dns.GetHostName(), Realname = Config.Realname });
            }

            return Connected;
        }

        public bool Disconnect()
        {
            IRC.Disconnect();
            Connected = false;

            return Connected;
        }

        private void HandleDisconnectEvent()
        {
            Connected = false;
        }
    }
}
