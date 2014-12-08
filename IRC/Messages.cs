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
        private Bot _bot;

        internal Messages(TCPInterface tcp, Bot bot)
        {
            _tcp = tcp;
            _bot = bot;
        }

        private void MessageParser()
        {
            while(_tcp.Connected)
            {
                string response = _tcp.Read();
            }
        }

        internal void SendMessage(Message message)
        {
            if (message.Type == MessageType.Channel || message.Type == MessageType.Query)
            {
                if (MessageSentEvent != null)
                {
                    MessageSentEvent(message);
                }
            }
        }

        internal void SendMessage(Nick nick, string message)
        {
            Message msg = new Message();
            msg.Receiver = nick;
            msg.Type = MessageType.Query;
            msg.Sender = _bot.Config.Nick;
            msg.Message = message;
            
            SendMessage(msg);
        }

        internal void SendMessage(Channel channel, string message)
        {
            Message msg = new Message();
            msg.Channel = channel;
            msg.Type = MessageType.Channel;
            msg.Sender = _bot.Config.Nick;
            msg.Message = message;
            
            SendMessage(msg);
        }

        internal void SendNotice(Message message)
        {
            if (message.Type == MessageType.Notice)
            {
                if (MessageSentEvent != null)
                {
                    MessageSentEvent(message);
                }
            }
        }

        internal void SendNotice(Nick nick, string message)
        {
            Message msg = new Message();
            msg.Receiver = nick;
            msg.Type = MessageType.Notice;
            msg.Sender = _bot.Config.Nick;
            msg.Message = message;
            
            SendNotice(msg);
        }

        internal void SendNotice(Channel channel, string message)
        {
            Message msg = new Message();
            msg.Channel = channel;
            msg.Type = MessageType.Notice;
            msg.Sender = _bot.Config.Nick;
            msg.Message = message;
            
            SendNotice(msg);
        }

        internal void SendCTCP(Message message)
        {
            if (message.Type == MessageType.CTCP)
            {
                if (MessageSentEvent != null)
                {
                    MessageSentEvent(message);
                }
            }
        }

        internal void SendCTCP(Nick nick, string message)
        {
            Message msg = new Message();
            msg.Receiver = nick;
            msg.Type = MessageType.CTCP;
            msg.Sender = _bot.Config.Nick;
            msg.Message = message;
            
            SendCTCP(msg);
        }

        internal void SendCTCP(Channel channel, string message)
        {
            Message msg = new Message();
            msg.Channel = channel;
            msg.Type = MessageType.CTCP;
            msg.Sender = _bot.Config.Nick;
            msg.Message = message;

            SendCTCP(msg);
        }
    }
}
