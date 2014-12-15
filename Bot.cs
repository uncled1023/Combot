using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combot
{
    class Bot
    {
        public event Action<BotError> ErrorEvent;

        public Config Config;

        public IRCService IRCService;

        internal Bot()
        {
            IRCService = new IRCService(this);

            Message msg = new Message();
            Nick nick = new Nick();
            msg.Sender = nick;
        }
    }
}
