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
using System.Windows;
using System.Windows.Documents;
using Combot;
using Combot.IRCServices.Messaging;
using Combot.IRCServices.Commanding;
using Combot.IRCServices;

namespace Interface.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
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
            ServerList = new ObservableCollection<string>();
            LocationList = new ObservableCollection<string>();
            BufferLock = new ReaderWriterLockSlim();

            Controller.Instance.Load();

            foreach (Bot Combot in Controller.Instance.Bots)
            {
                ServerList.Add(Combot.ServerConfig.Name);

                Combot.ErrorEvent += e => BotErrorHandler(e, Combot.ServerConfig.Name);

                // Incoming Messages
                Combot.IRC.Message.ErrorMessageEvent += (sender, e) => ErrorMessageHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.ServerReplyEvent += (sender, e) => ServerReplyHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.ChannelMessageReceivedEvent += (sender, e) => ChannelMessageReceivedHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.ChannelNoticeReceivedEvent += (sender, e) => ChannelNoticeReceivedHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.PrivateMessageReceivedEvent += (sender, e) => PrivateMessageReceivedHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.PrivateNoticeReceivedEvent += (sender, e) => PrivateNoticeReceivedHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.CTCPMessageReceivedEvent += (sender, e) => CTCPReceivedHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.CTCPNoticeReceivedEvent += (sender, e) => CTCPReceivedHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.JoinChannelEvent += (sender, e) => JoinEventHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.InviteChannelEvent += (sender, e) => InviteEventHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.PartChannelEvent += (sender, e) => PartEventHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.QuitEvent += (sender, e) => QuitEventHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.KickEvent += (sender, e) => KickEventHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.TopicChangeEvent += (sender, e) => TopicChangeEventHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.ChannelModeChangeEvent += (sender, e) => ChannelModeChangeHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.UserModeChangeEvent += (sender, e) => UserModeChangeHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Message.NickChangeEvent += (sender, e) => NickChangeHandler(sender, e, Combot.ServerConfig.Name);

                // Outgoing Messages
                Combot.IRC.Command.PrivateMessageCommandEvent += (sender, e) => PrivateMessageCommandHandler(sender, e, Combot.ServerConfig.Name);
                Combot.IRC.Command.PrivateNoticeCommandEvent += (sender, e) => PrivateNoticeCommandHandler(sender, e, Combot.ServerConfig.Name);

                Combot.IRC.ConnectEvent += () => ConnectHandler(Combot.ServerConfig.Name);
                Combot.IRC.DisconnectEvent += () => DisconnectHandler(Combot.ServerConfig.Name);
                Combot.IRC.TCPErrorEvent += e => TCPErrorHandler(e, Combot.ServerConfig.Name);

                SelectedServer = Combot.ServerConfig.Name;
            }

            Controller.Instance.AutoConnect();

            ToggleConnection = new DelegateCommand(ExecuteToggleConnection, CanToggleConnection);
            SubmitText = new DelegateCommand(ExecuteSubmitText, CanSubmitText);
            RemoveLocation = new DelegateCommand(ExecuteRemoveLocation, CanRemoveLocation);
            ClearLocation = new DelegateCommand(ExecuteClearLocation, CanClearLocation);
        }

        private void NickChangeHandler(object sender, NickChangeInfo e, string name)
        {
            string msg = string.Format(" * {0} is now known as {1}", e.OldNick.Nickname, e.NewNick.Nickname);
            AddToBuffer(name, null, string.Format("[{0}] {1}", e.TimeStamp.ToString("HH:mm:ss"), msg));
        }

        private void KickEventHandler(object sender, KickInfo e, string name)
        {
            string msg = string.Format(" * {1} has kicked {2} ({3})", e.Channel, e.Nick.Nickname, e.KickedNick.Nickname, e.Reason);
            AddToBuffer(name, e.Channel, string.Format("[{0}] {1}", e.TimeStamp.ToString("HH:mm:ss"), msg));
        }

        private void TopicChangeEventHandler(object sender, TopicChangeInfo e, string name)
        {
            string msg = string.Format(" * {1} has changed the topic to: {2}.", e.Channel, e.Nick.Nickname, e.Topic);
            AddToBuffer(name, e.Channel, string.Format("[{0}] {1}", e.TimeStamp.ToString("HH:mm:ss"), msg));
        }

        private void InviteEventHandler(object sender, InviteChannelInfo e, string name)
        {
            string msg = string.Format(" * {0} invited {1}", e.Requester.Nickname, e.Recipient.Nickname);
            AddToBuffer(name, e.Channel, string.Format("[{0}] {1}", e.TimeStamp.ToString("HH:mm:ss"), msg));
        }

        private void UserModeChangeHandler(object sender, UserModeChangeInfo e, string name)
        {
            string msg = string.Format(" * {0} sets mode {1}", e.Nick.Nickname, e.Modes.ModesToString());
            AddToBuffer(name, null, string.Format("[{0}] {1}", e.TimeStamp.ToString("HH:mm:ss"), msg));
        }

        private void ChannelModeChangeHandler(object sender, ChannelModeChangeInfo e, string name)
        {
            string msg = string.Format(" * {0} sets mode {1} on {2}.", e.Nick.Nickname, e.Modes.ModesToString(), e.Channel);
            AddToBuffer(name, e.Channel, string.Format("[{0}] {1}", e.TimeStamp.ToString("HH:mm:ss"), msg));
        }

        private void CTCPReceivedHandler(object sender, CTCPMessage e, string name)
        {
            string msg = string.Format("[CTCP] [{0}] {1}: {2}", e.Command, e.Sender.Nickname, e.Arguments);
            AddToBuffer(name, e.Location, string.Format("[{0}] {1}", e.TimeStamp.ToString("HH:mm:ss"), msg));
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
            string location = (SelectedServer == server) ? SelectedLocation : null;
            AddToBuffer(server, location, string.Format("[{0}] \u0002{1}\u0002 -NOTICE-: {2}", message.TimeStamp.ToString("HH:mm:ss"), message.Sender.Nickname, message.Message));
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
            string location = (SelectedServer == server) ? SelectedLocation : null;
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

        private void QuitEventHandler(object sender, QuitInfo info, string server)
        {
            string message = (info.Message == string.Empty) ? info.Nick.Nickname : info.Message;
            AddToBuffer(server, null, string.Format("[{0}] \u0002{1}\u0002 has quit: ({2})", info.TimeStamp.ToString("HH:mm:ss"), info.Nick.Nickname, message));
        }

        private void PrivateMessageCommandHandler(object sender, PrivateMessageCommand message, string server)
        {
            string curNick = string.Empty;
            Bot session = Controller.Instance.GetBot(SelectedServer);
            if (session != null)
            {
                curNick = session.IRC.Nickname;
            }
            AddToBuffer(server, message.Recipient, string.Format("[{0}] \u0002{1}\u0002: {2}", message.TimeStamp.ToString("HH:mm:ss"), curNick, message.Message));
        }

        private void PrivateNoticeCommandHandler(object sender, PrivateNoticeCommand message, string server)
        {
            string curNick = string.Empty;
            Bot session = Controller.Instance.GetBot(SelectedServer);
            if (session != null)
            {
                curNick = session.IRC.Nickname;
            }
            string location = (SelectedServer == server) ? SelectedLocation : null;
            AddToBuffer(server, location, string.Format("[{0}] \u0002{1}\u0002 -NOTICE-: {2}", message.TimeStamp.ToString("HH:mm:ss"), curNick, message.Message));
        }

        private void ConnectHandler(string server)
        {
            if (server == SelectedServer)
            {
                Connected = true;
            }
            AddToBuffer(server, null, "-- Connected --");
        }

        private void DisconnectHandler(string server)
        {
            if (server == SelectedServer)
            {
                Connected = false;
            }
            AddToBuffer(server, null, "-- Disconnected --");
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
            Bot botInstance = Controller.Instance.GetBot(SelectedServer);
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
                                botInstance.IRC.Command.SendPrivateMessage(SelectedLocation, message);
                            }
                            else
                            {
                                AddToBuffer(SelectedServer, SelectedLocation, "You are not in this channel.");
                            }
                        }
                        else
                        {
                            botInstance.IRC.Command.SendPrivateMessage(SelectedLocation, message);
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
                    Bot botInstance = Controller.Instance.GetBot(SelectedServer);
                    if (botInstance.IRC.Channels.Exists(chan => chan.Name == location))
                    {
                        botInstance.IRC.Command.SendPart(location);
                    }
                }
                if (LocationList.Contains(location))
                {
                    Application.Current.Dispatcher.Invoke((Action)(() => LocationList.Remove(location)));
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
            Controller.Instance.GetBot(server).Connect();
        }

        private void Disconnect(string server)
        {
            Controller.Instance.GetBot(server).Disconnect();
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
                Application.Current.Dispatcher.Invoke((Action) (() => LocationList.Add(location)));
            }
            BufferLock.EnterWriteLock();
            BufferInfo buffer = BufferList.Find(buf => buf.Server == server && buf.Location == location);
            if (buffer.Buffer.Count >= 500)
            {
                buffer.Buffer.RemoveAt(0);
            }
            buffer.Buffer.Add(message);
            BufferLock.ExitWriteLock();
            ChangeBuffer();
        }

        private void ChangeServer()
        {
            Application.Current.Dispatcher.Invoke((Action)(() => LocationList.Clear()));
            for (int i = 0; i < BufferList.Count; i++)
            {
                if (BufferList[i].Server == SelectedServer)
                {
                    int index = i;
                    Application.Current.Dispatcher.Invoke((Action)(() => LocationList.Add(BufferList[index].Location)));
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
                Bot session = Controller.Instance.GetBot(SelectedServer);
                if (session != null)
                {
                    Connected = session.Connected;
                }
            }
        }
    }
}
