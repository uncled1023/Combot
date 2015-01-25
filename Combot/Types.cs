using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net;

namespace Combot
{
    public enum ErrorType
    {
        Bot = 0,
        TCP = 1,
        IRC = 2,
        Framework = 3
    }

    public enum MessageType
    {
        Service = 0,
        Channel = 1,
        Query = 2,
        Notice = 3,
        CTCP = 4
    }

    public class BotError
    {
        public ErrorType Type { get; set; }
        public string Message { get; set; }
    }

    public class Server
    {
        public string Name { get; set; }
        public List<IPEndPoint> Hosts { get; set; }
        public List<string> Channels { get; set; }
        public bool AutoConnect { get; set; }
    }
}
