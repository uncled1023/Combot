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
        private TCPInterface _tcp;

        internal IRCService()
        {
            _tcp = new TCPInterface();
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

            return result;
        }
    }
}
