using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Windows.Documents;
using Combot;
using Combot.IRCServices.Messaging;
using Combot.Configurations;

namespace Interface.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public List<Bot> CombotSessions = new List<Bot>();
        public Config Config = new Config();

        public string ApplicationTitle { get; set; }

        private string _CurrentBuffer = string.Empty;

        public string CurrentBuffer
        {
            get { return _CurrentBuffer; }
            set
            {
                _CurrentBuffer = value;
                OnPropertyChanged("CurrentBuffer");
            }
        }

        private bool _Connected = false;

        public bool Connected
        {
            get { return _Connected; }
            set
            {
                _Connected = value;
                if (_Connected)
                {
                    ToggleConnectionText = "Disconnect";
                }
                else
                {
                    ToggleConnectionText = "Connect";
                }
                OnPropertyChanged("Connected");
            }
        }

        private string _ToggleConnectionText = "Connect";

        public string ToggleConnectionText
        {
            get { return _ToggleConnectionText; }
            set
            {
                _ToggleConnectionText = value;
                OnPropertyChanged("ToggleConnectionText");
            }
        }

        private string _InputBoxText;

        public string InputBoxText
        {
            get { return _InputBoxText; }
            set
            {
                _InputBoxText = value;
                OnPropertyChanged("InputBoxText");
            }
        }

        private string _SelectedServer;
        public string SelectedServer
        {
            get { return _SelectedServer; }
            set
            {
                _SelectedServer = value;
                OnPropertyChanged("SelectedServer");
                ChangeServer();
                ChangeBuffer();
            }
        }

        private string _SelectedLocation;

        public string SelectedLocation
        {
            get { return _SelectedLocation; }
            set
            {
                _SelectedLocation = value;
                OnPropertyChanged("SelectedLocation");
                ChangeBuffer();
            }
        }

        private ObservableCollection<string> _ServerList;

        public ObservableCollection<string> ServerList
        {
            get { return _ServerList; }
            set
            {
                if (_ServerList != value)
                {
                    _ServerList = value;
                    OnPropertyChanged("ServerList");
                }
            }
        }

        private ObservableCollection<string> _LocationList;

        public ObservableCollection<string> LocationList
        {
            get { return _LocationList; }
            set
            {
                if (_LocationList != value)
                {
                    _LocationList = value;
                    OnPropertyChanged("LocationList");
                }
            }
        }

        public DelegateCommand ToggleConnection { get; private set; }
        public DelegateCommand SubmitText { get; set; }

        private List<BufferInfo> BufferList = new List<BufferInfo>();

        public MainViewModel()
        {
            ApplicationTitle = "Combot";
            Config.LoadServers();
            ServerList = new ObservableCollection<string>();
            LocationList = new ObservableCollection<string>();

            foreach (ServerConfig server in Config.Servers)
            {
                ServerList.Add(server.Name);
                Bot Combot = new Bot(server);

                Combot.IRC.Message.ErrorMessageEvent += (sender, e) => ErrorMessageHandler(sender, e, server.Name);
                Combot.IRC.Message.ServerReplyEvent += (sender, e) => ServerReplyHandler(sender, e, server.Name);
                Combot.IRC.Message.ChannelMessageReceivedEvent += (sender, e) => ChannelMessageReceivedHandler(sender, e, server.Name);
                Combot.IRC.Message.ChannelNoticeReceivedEvent += (sender, e) => ChannelNoticeReceivedHandler(sender, e, server.Name);
                Combot.IRC.Message.PrivateMessageReceivedEvent += (sender, e) => PrivateMessageReceivedHandler(sender, e, server.Name);
                Combot.IRC.Message.PrivateNoticeReceivedEvent += (sender, e) => PrivateNoticeReceivedHandler(sender, e, server.Name);
                //Combot.IRC.Message.RawMessageEvent += RawMessageHandler;

                Combot.IRC.Message.JoinChannelEvent += (sender, e) => JoinEventHandler(sender, e, server.Name);

                Combot.IRC.ConnectEvent += () => ConnectHandler(server.Name);
                Combot.IRC.DisconnectEvent += () => DisconnectHandler(server.Name);
                Combot.IRC.TCPErrorEvent += e => TCPErrorHandler(e, server.Name);

                CombotSessions.Add(Combot);
                SelectedServer = server.Name;

                if (server.AutoConnect)
                {
                    Combot.Connect();
                }
            }

            ToggleConnection = new DelegateCommand(ExecuteToggleConnection, CanToggleConnection);
            SubmitText = new DelegateCommand(ExecuteSubmitText, CanSubmitText);
        }

        private void JoinEventHandler(object sender, JoinChannelInfo info, string server)
        {
            AddToBuffer(server, info.Channel, string.Format("{0} has joined {1}.", info.Nick.Nickname, info.Channel));
        }

        private void RawMessageHandler(object sender, string message)
        {
            //CurrentBuffer += message + Environment.NewLine;
        }

        private void TCPErrorHandler(Combot.IRCServices.TCP.TCPError error, string server)
        {
            AddToBuffer(server, null, string.Format("[{0}] {1}", error.Code.ToString(), error.Message));
        }

        private void ServerReplyHandler(object sender, IReply reply, string server)
        {
            AddToBuffer(server, null, reply.Message);
        }

        private void ErrorMessageHandler(object sender, ErrorMessage message, string server)
        {
            AddToBuffer(server, null, message.Message);
        }

        private void ChannelMessageReceivedHandler(object sender, ChannelMessage message, string server)
        {
            AddToBuffer(server, message.Channel, message.Message);
        }

        private void ChannelNoticeReceivedHandler(object sender, ChannelNotice message, string server)
        {
            AddToBuffer(server, message.Channel, message.Message);
        }

        private void PrivateMessageReceivedHandler(object sender, PrivateMessage message, string server)
        {
            AddToBuffer(server, message.Sender.Nickname, message.Message);
        }

        private void PrivateNoticeReceivedHandler(object sender, PrivateNotice message, string server)
        {
            AddToBuffer(server, message.Sender.Nickname, message.Message);
        }

        private void ConnectHandler(string server)
        {
            if (server == SelectedServer)
            {
                Connected = true;
            }
        }

        private void DisconnectHandler(string server)
        {
            if (server == SelectedServer)
            {
                Connected = false;
            }
        }

        private void ExecuteToggleConnection()
        {
            if (_Connected)
            {
                Disconnect(SelectedServer);
            }
            else
            {
                Connect(SelectedServer);
            }
        }

        private bool CanToggleConnection()
        {
            return true;
        }

        private void ExecuteSubmitText()
        {
            if (SelectedLocation != " --Server-- ")
            {
                Bot botInstance = CombotSessions.Find(bot => bot.ServerConfig.Name == SelectedServer);
                if (botInstance != null && botInstance.Connected)
                {
                    string message = InputBoxText;
                    if (InputBoxText.StartsWith("/"))
                    {
                        MessageType type = MessageType.Query;
                        if (SelectedLocation.StartsWith("#") || SelectedLocation.StartsWith("&"))
                        {
                            type = MessageType.Channel;
                        }
                        message = message.Remove(0, 1);
                        message = string.Join("", botInstance.ServerConfig.CommandPrefix, message);
                        botInstance.ExecuteCommand(message, SelectedLocation, type);
                    }
                    else
                    {
                        botInstance.IRC.SendPrivateMessage(SelectedLocation, message);
                    }
                    InputBoxText = string.Empty;
                }
            }
        }

        private bool CanSubmitText()
        {
            return true;
        }

        private void Connect(string server)
        {
            CombotSessions.Find(bot => bot.ServerConfig.Name == server).Connect();
        }

        private void Disconnect(string server)
        {
            CombotSessions.Find(bot => bot.ServerConfig.Name == server).Disconnect();
        }

        private void AddToBuffer(string server, string location, string message)
        {
            if (location == null)
            {
                location = " --Server-- ";
            }
            if (!BufferList.Exists(buf => buf.Server == server && buf.Location == location))
            {
                BufferInfo newBuffer = new BufferInfo();
                newBuffer.Server = server;
                newBuffer.Location = location;
                BufferList.Add(newBuffer);
            }
            if (SelectedServer == server && !LocationList.Contains(location))
            {
                App.Current.Dispatcher.Invoke((Action) (() => LocationList.Add(location)));
            }
            BufferInfo buffer = BufferList.Find(buf => buf.Server == server && buf.Location == location);
            if (buffer.Buffer.Count >= 1000)
            {
                buffer.Buffer.RemoveAt(0);
            }
            buffer.Buffer.Add(message);
            ChangeBuffer();
        }

        private void ChangeServer()
        {
            App.Current.Dispatcher.Invoke((Action)(() => LocationList.Clear()));
            for (int i = 0; i < BufferList.Count; i++)
            {
                if (BufferList[i].Server == SelectedServer)
                {
                    App.Current.Dispatcher.Invoke((Action)(() => LocationList.Add(BufferList[i].Location)));
                }
            }
            if (LocationList.Any())
            {
                SelectedLocation = LocationList.First();
            }
        }

        private void ChangeBuffer()
        {
            if (SelectedServer != null && SelectedLocation != null)
            {
                if (!BufferList.Exists(buf => buf.Server == SelectedServer && buf.Location == SelectedLocation))
                {
                    BufferInfo newBuffer = new BufferInfo();
                    newBuffer.Server = SelectedServer;
                    newBuffer.Location = SelectedLocation;
                    BufferList.Add(newBuffer);
                }
                CurrentBuffer = string.Join(Environment.NewLine, BufferList.Find(buf => buf.Server == SelectedServer && buf.Location == SelectedLocation).Buffer);
                Connected = CombotSessions.Find(bot => bot.ServerConfig.Name == SelectedServer).Connected;
            }
        }
    }
}
