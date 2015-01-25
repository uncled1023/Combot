using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Combot.IRCServices.Messaging;
using Combot.IRCServices.TCP;

namespace Combot.IRCServices
{
    public partial class IRC
    {
        public bool Connected;
        public List<Channel> Channels = new List<Channel>();
        public Messages Message;
        public event Action DisconnectEvent;

        private TCPInterface _TCP;
        private Thread TCPReader;
        private event Action<string> TCPMessageEvent;

        public IRC()
        {
            Connected = false;
            _TCP = new TCPInterface();
            Message = new Messages(this);
            TCPMessageEvent += Message.ParseTCPMessage;
            Message.ErrorMessageEvent += HandleErrorMessage;
            Message.PingEvent += HandlePing;
        }

        public bool Connect(IPAddress IP, int port, int readTimeout = 5000, int allowedFailedCount = 0)
        {
            bool result = false;
            if (!_TCP.Connected)
            {
                result = _TCP.Connect(IP, port, readTimeout, allowedFailedCount);
                if (result)
                {
                    TCPReader = new Thread(ReadTCPMessages);
                    TCPReader.IsBackground = true;
                    TCPReader.Start();
                }
            }

            return result;
        }

        public bool Disconnect()
        {
            bool result = false;

            if (_TCP.Connected)
            {
                _TCP.Disconnect();
            }

            if (DisconnectEvent != null)
            {
                DisconnectEvent();
            }

            return result;
        }

        public void Login(string serverName, Nick nick)
        {
            IRCSendNick(nick.Nickname);
            IRCSendUser(nick.Nickname, nick.Host, serverName, nick.Realname);
        }

        private void ReadTCPMessages()
        {
            while (_TCP.Connected)
            {
                string response = ReadTCPMessage();
                if (TCPMessageEvent != null && response != null && response != string.Empty)
                {
                    TCPMessageEvent(response);
                }

                Thread.Sleep(10);
            }
        }

        private string ReadTCPMessage()
        {
            if (_TCP.Connected)
            {
                return _TCP.Read();
            }
            return null;
        }

        private void SendTCPMessage(string message)
        {
            if (_TCP.Connected)
            {
                _TCP.Write(message);
            }
        }

        private void HandleErrorMessage(object sender, IRCServices.Messaging.ErrorMessage e)
        {
            Disconnect();
        }

        private void HandlePing(object sender, IRCServices.Messaging.PingInfo e)
        {
            IRCSendPong(e.Message);
        }
    }
}
