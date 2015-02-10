using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Windows.Documents;
using Combot;
using Combot.IRCServices.Messaging;
using Combot.Configurations;
using Combot.IRCServices;

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
        public DelegateCommand RemoveLocation { get; set; }
        public DelegateCommand ClearLocation { get; set; }

        private List<BufferInfo> BufferList = new List<BufferInfo>();
        private ReaderWriterLockSlim BufferLock;

        public MainViewModel()
        {
            ApplicationTitle = "Combot";
            Config.LoadServers();
            ServerList = new ObservableCollection<string>();
            LocationList = new ObservableCollection<string>();
            BufferLock = new ReaderWriterLockSlim();

            foreach (ServerConfig server in Config.Servers)
            {
                ServerList.Add(server.Name);
                Bot Combot = new Bot(server);

                Combot.ErrorEvent += e => BotErrorHandler(e, Combot.ServerConfig.Name);
                Combot.IRC.Message.ErrorMessageEvent += (sender, e) => ErrorMessageHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.ServerReplyEvent += (sender, e) => ServerReplyHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.ChannelMessageReceivedEvent += (sender, e) => ChannelMessageReceivedHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.ChannelNoticeReceivedEvent += (sender, e) => ChannelNoticeReceivedHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.PrivateMessageReceivedEvent += (sender, e) => PrivateMessageReceivedHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.PrivateNoticeReceivedEvent += (sender, e) => PrivateNoticeReceivedHandler(sender, e, Combot.ServerConfig.Name);

                Combot.IRC.Message.JoinChannelEvent += (sender, e) => JoinEventHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.PartChannelEvent += (sender, e) => PartEventHandler(sender, e, Combot.ServerConfig.Name);

                Combot.IRC.ConnectEvent += () => ConnectHandler(Combot.ServerConfig.Name);
                Combot.IRC.DisconnectEvent += () => DisconnectHandler(Combot.ServerConfig.Name);
                Combot.IRC.TCPErrorEvent += e => TCPErrorHandler(e, Combot.ServerConfig.Name);

                CombotSessions.Add(Combot);
                SelectedServer = server.Name;

                if (server.AutoConnect)
                {
                    Combot.Connect();
                }
            }

            ToggleConnection = new DelegateCommand(ExecuteToggleConnection, CanToggleConnection);
            SubmitText = new DelegateCommand(ExecuteSubmitText, CanSubmitText);
            RemoveLocation = new DelegateCommand(ExecuteRemoveLocation, CanRemoveLocation);
            ClearLocation = new DelegateCommand(ExecuteClearLocation, CanClearLocation);
        }

        private void BotErrorHandler(BotError error, string server)
        {
            AddToBuffer(server, null, string.Format("[{0}] \u0002{1} Error\u0002: {2}", DateTime.Now.ToString("HH:mm:ss"), error.Type, error.Message));
        }

        private void TCPErrorHandler(Combot.IRCServices.TCP.TCPError error, string server)
        {
            AddToBuffer(server, null, string.Format("[{0}] \u0002TCP Error {1}\u0002: {2}", DateTime.Now.ToString("HH:mm:ss"), error.Code, error.Message));
        }

        private void ServerReplyHandler(object sender, IReply reply, string server)
        {
            AddToBuffer(server, null, string.Format("[{0}] \u0002*\u0002: {1}", reply.TimeStamp.ToString("HH:mm:ss"), reply.Message));
        }

        private void ErrorMessageHandler(object sender, ErrorMessage message, string server)
        {
            AddToBuffer(server, null, string.Format("[{0}] \u0002*\u0002: {1}", message.TimeStamp.ToString("HH:mm:ss"), message.Message));
        }

        private void ChannelMessageReceivedHandler(object sender, ChannelMessage message, string server)
        {
            AddToBuffer(server, message.Channel, string.Format("[{0}] \u0002{1}\u0002: {2}", message.TimeStamp.ToString("HH:mm:ss"), message.Sender.Nickname, message.Message));
        }

        private void ChannelNoticeReceivedHandler(object sender, ChannelNotice message, string server)
        {
            AddToBuffer(server, message.Channel, string.Format("[{0}] \u0002{1}\u0002 -NOTICE-: {2}", message.TimeStamp.ToString("HH:mm:ss"), message.Sender.Nickname, message.Message));
        }

        private void PrivateMessageReceivedHandler(object sender, PrivateMessage message, string server)
        {
            string location = message.Sender.Nickname;
            if (message.Sender.Nickname.ToLower() == "nickserv" || message.Sender.Nickname.ToLower() == "chanserv")
            {
                location = null;
            }
            AddToBuffer(server, location, string.Format("[{0}] \u0002{1}\u0002: {2}", message.TimeStamp.ToString("HH:mm:ss"), message.Sender.Nickname, message.Message));
        }

        private void PrivateNoticeReceivedHandler(object sender, PrivateNotice message, string server)
        {
            string location = message.Sender.Nickname;
            if (message.Sender.Nickname.ToLower() == "nickserv" || message.Sender.Nickname.ToLower() == "chanserv")
            {
                location = null;
            }
            AddToBuffer(server, location, string.Format("[{0}] \u0002{1}\u0002 -NOTICE-: {2}", message.TimeStamp.ToString("HH:mm:ss"), message.Sender.Nickname, message.Message));
        }

        private void JoinEventHandler(object sender, JoinChannelInfo info, string server)
        {
            AddToBuffer(server, info.Channel, string.Format("[{0}] \u0002{1}\u0002 has joined \u0002{2}\u0002.", info.TimeStamp.ToString("HH:mm:ss"), info.Nick.Nickname, info.Channel));
        }

        private void PartEventHandler(object sender, PartChannelInfo info, string server)
        {
            AddToBuffer(server, info.Channel, string.Format("[{0}] \u0002{1}\u0002 has left \u0002{2}\u0002.", info.TimeStamp.ToString("HH:mm:ss"), info.Nick.Nickname, info.Channel));
        }

        private void ConnectHandler(string server)
        {
            if (server == SelectedServer)
            {
                Connected = true;
                AddToBuffer(server, null, "-- Connected --");
            }
        }

        private void DisconnectHandler(string server)
        {
            if (server == SelectedServer)
            {
                Connected = false;
                AddToBuffer(server, null, "-- Disconnected --");
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
                    if (SelectedLocation != " --Server-- ")
                    {
                        if (SelectedLocation.StartsWith("#") || SelectedLocation.StartsWith("&"))
                        {
                            if (botInstance.IRC.Channels.Exists(chan => chan.Name == SelectedLocation))
                            {
                                botInstance.IRC.SendPrivateMessage(SelectedLocation, message);
                            }
                            else
                            {
                                AddToBuffer(SelectedServer, SelectedLocation, "You are not in this channel.");
                            }
                        }
                        else
                        {
                            botInstance.IRC.SendPrivateMessage(SelectedLocation, message);
                        }
                    }
                }
                InputBoxText = string.Empty;
            }
        }

        private bool CanSubmitText()
        {
            if (Connected)
            {
                return true;
            }
            return false;
        }

        private void ExecuteRemoveLocation()
        {
            if (SelectedLocation != " --Server-- ")
            {
                string location = SelectedLocation;

                if (location.StartsWith("#") || location.StartsWith("&"))
                {
                    Bot botInstance = CombotSessions.Find(bot => bot.ServerConfig.Name == SelectedServer);
                    if (botInstance.IRC.Channels.Exists(chan => chan.Name == location))
                    {
                        botInstance.IRC.SendPart(location);
                    }
                }
                if (LocationList.Contains(location))
                {
                    App.Current.Dispatcher.Invoke((Action)(() => LocationList.Remove(location)));
                }
                BufferLock.EnterWriteLock();
                if (BufferList.Exists(buf => buf.Server == SelectedServer && buf.Location == location))
                {
                    BufferList.RemoveAll(buf => buf.Server == SelectedServer && buf.Location == location);
                }
                BufferLock.ExitWriteLock();
            }
        }

        private bool CanRemoveLocation()
        {
            return true;
        }

        private void ExecuteClearLocation()
        {
            string location = SelectedLocation;
            BufferLock.EnterWriteLock();
            if (BufferList.Exists(buf => buf.Server == SelectedServer && buf.Location == location))
            {
                BufferList.Find(buf => buf.Server == SelectedServer && buf.Location == location).Buffer.Clear();
            }
            BufferLock.ExitWriteLock();
            CurrentBuffer = string.Empty;
        }

        private bool CanClearLocation()
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
            BufferLock.EnterWriteLock();
            if (!BufferList.Exists(buf => buf.Server == server && buf.Location == location))
            {
                BufferInfo newBuffer = new BufferInfo();
                newBuffer.Server = server;
                newBuffer.Location = location;
                BufferList.Add(newBuffer);
            }
            BufferLock.ExitWriteLock();
            if (SelectedServer == server && !LocationList.Contains(location))
            {
                App.Current.Dispatcher.Invoke((Action) (() => LocationList.Add(location)));
            }
            BufferLock.EnterWriteLock();
            BufferInfo buffer = BufferList.Find(buf => buf.Server == server && buf.Location == location);
            if (buffer.Buffer.Count >= 1000)
            {
                buffer.Buffer.RemoveAt(0);
            }
            buffer.Buffer.Add(message);
            BufferLock.ExitWriteLock();
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
                SelectedLocation = " --Server-- ";
            }
        }

        private void ChangeBuffer()
        {
            if (SelectedServer != null && SelectedLocation != null)
            {
                BufferLock.EnterWriteLock();
                if (!BufferList.Exists(buf => buf.Server == SelectedServer && buf.Location == SelectedLocation))
                {
                    BufferInfo newBuffer = new BufferInfo();
                    newBuffer.Server = SelectedServer;
                    newBuffer.Location = SelectedLocation;
                    BufferList.Add(newBuffer);
                }
                CurrentBuffer = string.Join(Environment.NewLine, BufferList.Find(buf => buf.Server == SelectedServer && buf.Location == SelectedLocation).Buffer);
                BufferLock.ExitWriteLock();
                Connected = CombotSessions.Find(bot => bot.ServerConfig.Name == SelectedServer).Connected;
            }
        }
    }
}
