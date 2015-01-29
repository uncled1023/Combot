using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Combot;
using Combot.IRCServices.Messaging;
using Combot.Configurations;
using Combot.Modules;
using Combot.Modules.ModuleClasses;

namespace Interface.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public List<Bot> CombotSessions = new List<Bot>();
        public Config Config = new Config();

        public string ApplicationTitle { get; set; }

        private string _CurrentBuffer = string.Empty;
        public string CurrentBuffer { get { return _CurrentBuffer; } set { _CurrentBuffer = value; OnPropertyChanged("CurrentBuffer"); } }

        private bool _Connected = false;
        public bool Connected { get { return _Connected; } set { _Connected = value; if (_Connected) { ToggleConnectionText = "Disconnect"; } else { ToggleConnectionText = "Connect"; } OnPropertyChanged("Connected"); } }

        private string _ToggleConnectionText = "Connect";
        public string ToggleConnectionText { get { return _ToggleConnectionText; } set { _ToggleConnectionText = value; OnPropertyChanged("ToggleConnectionText"); } }

        public DelegateCommand ToggleConnection { get; private set; }

        public MainViewModel()
        {
            ApplicationTitle = "Combot";
            Config.LoadServers();
            /*
            ServerConfig serverConfig = new ServerConfig();
            serverConfig.AutoConnect = true;
            serverConfig.Channels = new List<ChannelConfig> { new ChannelConfig() { Name = "#testing", Key = string.Empty } };
            serverConfig.Name = "Rizon";
            serverConfig.Nickname = "Combot_V3";
            serverConfig.Realname = "Combot_Realname";
            serverConfig.Username = "Combot_Username";
            serverConfig.Hosts = new List<HostConfig> { new HostConfig() { Host = "irc.rizon.net", Port = 6667 } };
            serverConfig.Modules = new List<Module>
            {
                new Module
                {
                    Name = "Ping Me",
                    ClassName = "PingMe",
                    Enabled = true,
                    Commands = new List<Command> { new Command { Name = "Ping Me", Triggers = new List<string>() {"pingme"} } }
                }
            };
            Config.Servers.Add(serverConfig);
            Config.SaveServers();
            */
            foreach (ServerConfig server in Config.Servers)
            {
                Bot Combot = new Bot(server);

                Combot.IRC.Message.ErrorMessageEvent += ErrorMessageHandler;
                Combot.IRC.Message.ServerReplyEvent += ServerReplyHandler;
                Combot.IRC.Message.ChannelMessageReceivedEvent += ChannelMessageReceivedHandler;
                Combot.IRC.Message.ChannelNoticeReceivedEvent += ChannelNoticeReceivedHandler;
                Combot.IRC.Message.PrivateMessageReceivedEvent += PrivateMessageReceivedHandler;
                Combot.IRC.Message.PrivateNoticeReceivedEvent += PrivateNoticeReceivedHandler;

                Combot.IRC.ConnectEvent += ConnectHandler;
                Combot.IRC.DisconnectEvent += DisconnectHandler;
                Combot.IRC.TCPErrorEvent += TCPErrorHandler;

                CombotSessions.Add(Combot);
            }

            ToggleConnection = new DelegateCommand(ExecuteToggleConnection, CanToggleConnection);
        }

        private void TCPErrorHandler(Combot.IRCServices.TCP.TCPError error)
        {
            CurrentBuffer += string.Format("[{0}] {1}", error.Code.ToString(), error.Message) + Environment.NewLine;
        }

        private void ServerReplyHandler(object sender, IReply reply)
        {
            CurrentBuffer += reply.Message + Environment.NewLine;
        }

        private void ErrorMessageHandler(object sender, ErrorMessage message)
        {
            CurrentBuffer += message.Message + Environment.NewLine;
        }

        private void ChannelMessageReceivedHandler(object sender, ChannelMessage message)
        {
            CurrentBuffer += message.Message + Environment.NewLine;
        }

        private void ChannelNoticeReceivedHandler(object sender, ChannelNotice message)
        {
            CurrentBuffer += message.Message + Environment.NewLine;
        }

        private void PrivateMessageReceivedHandler(object sender, PrivateMessage message)
        {
            CurrentBuffer += message.Message + Environment.NewLine;
        }

        private void PrivateNoticeReceivedHandler(object sender, PrivateNotice message)
        {
            CurrentBuffer += message.Message + Environment.NewLine;
        }

        private void ConnectHandler()
        {
            Connected = true;
        }

        private void DisconnectHandler()
        {
            Connected = false;
        }

        private void ExecuteToggleConnection()
        {
            if (_Connected)
            {
                Disconnect();
            }
            else
            {
                Connect();
            }
        }

        private bool CanToggleConnection()
        {
            return true;
        }

        private void Connect()
        {
            CombotSessions.ForEach(Combot => Combot.Connect());
        }

        private void Disconnect()
        {
            CombotSessions.ForEach(Combot => Combot.Disconnect());
        }
    }
}
