using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combot
{
    class Messages
    {
        public event Action<Message> MessageReceivedEvent;
        public event Action<Message> MessageSentEvent;

        private TCPInterface _tcp;
        private static Queue<Message> _messageQueue;

        internal Messages(TCPInterface tcp)
        {
            _tcp = tcp;
            _messageQueue = new Queue<Message>();
        }


    }
}
