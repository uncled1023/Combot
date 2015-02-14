using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Combot;
using Combot.IRCServices.Messaging;
using Combot.Configurations;
using Combot.IRCServices;
using Combot.IRCServices.Commanding;

namespace Console_Interface
{
    public class Console_Interface
    {
        public static List<Bot> CombotSessions = new List<Bot>();
        public static Config Config = new Config();

        public static void Main(string[] args)
        {
            Config.LoadServers();

            foreach (ServerConfig server in Config.Servers)
            {
                Bot Combot = new Bot(server);

                Combot.ErrorEvent += e => BotErrorHandler(e, Combot.ServerConfig.Name);

                // Incoming Messages
                Combot.IRC.Message.RawMessageEvent += (sender, e) => RawMessageHandler(sender, e, Combot.ServerConfig.Name);

                // Outgoing Messages
                Combot.IRC.Command.PrivateMessageCommandEvent += (sender, e) => PrivateMessageCommandHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Command.PrivateNoticeCommandEvent += (sender, e) => PrivateNoticeCommandHandler(sender, e, Combot.ServerConfig.Name);

                Combot.IRC.ConnectEvent += () => ConnectHandler(Combot.ServerConfig.Name);
                Combot.IRC.DisconnectEvent += () => DisconnectHandler(Combot.ServerConfig.Name);
                Combot.IRC.TCPErrorEvent += e => TCPErrorHandler(e, Combot.ServerConfig.Name);

                CombotSessions.Add(Combot);

                if (server.AutoConnect)
                {
                    Combot.Connect();
                }
            }

            bool run = true;
            while (run)
            {
                ConsoleKeyInfo info = Console.ReadKey();
                if (info.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("Exiting...");
                    run = false;
                }
            }
        }

        private static void RawMessageHandler(object sender, string message, string server)
        {
            string msg = string.Format("[{0}] [{1}] {2}", DateTime.Now.ToString("HH:mm:ss"), server, message);
            Console.WriteLine(msg);
        }

        private static void BotErrorHandler(BotError error, string server)
        {
            string message = string.Format("[{0}] [{1}] \u0002{2} Error\u0002: {3}", DateTime.Now.ToString("HH:mm:ss"), server, error.Type, error.Message);
            Console.WriteLine(message);
        }

        private static void TCPErrorHandler(Combot.IRCServices.TCP.TCPError error, string server)
        {
            string message = string.Format("[{0}] [{1}] \u0002TCP Error {2}\u0002: {3}", DateTime.Now.ToString("HH:mm:ss"), server, error.Code, error.Message);
            Console.WriteLine(message);
        }

        private static void PrivateMessageCommandHandler(object sender, PrivateMessageCommand message, string server)
        {
            string msg = string.Format("[{0}] [{1}] \u0002{2}\u0002: {3}", message.TimeStamp.ToString("HH:mm:ss"), server, " --Combot-- ", message.Message);
            Console.WriteLine(msg);
        }

        private static void PrivateNoticeCommandHandler(object sender, PrivateNoticeCommand message, string server)
        {
            string msg = string.Format("[{0}] [{1}] \u0002{2}\u0002 -NOTICE-: {3}", message.TimeStamp.ToString("HH:mm:ss"), server, " --Combot-- ", message.Message);
            Console.WriteLine(msg);
        }

        private static void ConnectHandler(string server)
        {
            Console.WriteLine("-- {0} Connected --", server);
        }

        private static void DisconnectHandler(string server)
        {
            Console.WriteLine("-- {0} Disconnected --", server);
        }
    }
}
