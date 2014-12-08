using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Combot
{
    internal class IRCService
    {
        public Action<BotError> ErrorEvent;

        private Bot _bot;
        private TCPInterface _tcp;
        private Messages _messages;

        internal IRCService(Bot bot)
        {
            _bot = bot;
            _tcp = new TCPInterface();
            _messages = new Messages(_tcp, _bot);
        }

        internal bool Connect(IPAddress IP, int port, int readTimeout, int allowedFailedCount = 0)
        {
            bool result = false;
            if (!_tcp.Connected)
            {
                result = _tcp.Connect(IP, port, readTimeout, allowedFailedCount);
            }

            return result;
        }

        internal bool Disconnect()
        {
            bool result = false;

            if (_tcp.Connected)
            {
                _tcp.Disconnect();
            }

            return result;
        }
    }
}
