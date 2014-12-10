using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace Combot
{
    internal partial class IRCService
    {
        protected Bot _Bot;
        protected TCPInterface _TCP;
        protected Thread TCPReader;
        protected event Action<string> TCPMessageEvent;

        internal IRCService(Bot bot)
        {
            _Bot = bot;
            _TCP = new TCPInterface();
            TCPMessageEvent += ParseTCPMessage;
        }

        internal bool Connect(IPAddress IP, int port, int readTimeout, int allowedFailedCount = 0)
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

        internal bool Disconnect()
        {
            bool result = false;

            if (_TCP.Connected)
            {
                _TCP.Disconnect();
            }

            return result;
        }

        protected void ReadTCPMessages()
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

        protected string ReadTCPMessage()
        {
            if (_TCP.Connected)
            {
                return _TCP.Read();
            }
            return null;
        }

        protected void SendTCPMessage(string message)
        {
            if (_TCP.Connected)
            {
                _TCP.Write(message);
            }
        }
    }
}
