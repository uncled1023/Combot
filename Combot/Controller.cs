using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Combot.Configurations;

namespace Combot
{
    public class Controller
    {
        public static Controller Instance { get { return GetInstance(); } }

        public static Controller GetInstance() { return _controller ?? (_controller = new Controller()); }

        private static Controller _controller;

        private List<Bot> _bots;
        public List<Bot> Bots { get; private set; }
        public readonly Config Config = new Config();

        private Controller()
        {
            Load();
        }

        public void Load()
        {
            Config.LoadServers();
            Bots = new List<Bot>();

            foreach (ServerConfig server in Config.Servers)
            {
                Bot Combot = new Bot(server);
                Bots.Add(Combot);
            }
        }

        public void AutoConnect()
        {
            Bots.ForEach(bot =>
            {
                if (bot.ServerConfig.AutoConnect) { bot.Connect(); } 
            });
        }

        public Bot GetBot(string server)
        {
            return Bots.Find(bot => bot.ServerConfig.Name == server);
        }
    }
}