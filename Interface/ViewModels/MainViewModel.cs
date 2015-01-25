using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Combot;
using Combot.IRCServices.Messaging;

namespace Interface.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public Bot Combot = new Bot();

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
            Combot.Config.Nick = "Combot_V3";
            Combot.Config.Realname = "Combot_V3_realname";
            Combot.Config.Server = new Server();
            Combot.Config.Server.AutoConnect = true;
            Combot.Config.Server.Channels = new List<string>() { "#testing" };
            Combot.Config.Server.Name = "Rizon";
            IPAddress[] ipList = Dns.GetHostAddresses("irc.rizon.net");
            Combot.Config.Server.Hosts = new List<IPEndPoint>();
            foreach (IPAddress ip in ipList)
            {
                Combot.Config.Server.Hosts.Add(new System.Net.IPEndPoint(ip, 6667));
            }

            Combot.IRC.Message.ErrorMessageEvent += ErrorMessageHandler;
            Combot.IRC.Message.ServerReplyEvent += ServerReplyHandler;
            Combot.IRC.Message.ChannelMessageReceivedEvent += ChannelMessageReceivedHandler;
            Combot.IRC.Message.ChannelNoticeReceivedEvent += ChannelNoticeReceivedHandler;
            Combot.IRC.Message.PrivateMessageReceivedEvent += PrivateMessageReceivedHandler;
            Combot.IRC.Message.PrivateNoticeReceivedEvent += PrivateNoticeReceivedHandler;

            Combot.IRC.DisconnectEvent += DisconnectHandler;

            ToggleConnection = new DelegateCommand(ExecuteToggleConnection, CanToggleConnection);
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
            Connected = Combot.Connect();
            if (Connected)
            {
                Combot.Login();
            }
        }

        private void Disconnect()
        {
            Connected = Combot.Disconnect();
        }
    }
}
