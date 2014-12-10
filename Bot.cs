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

        internal IRCService _ircService;

        internal Bot()
        {
            _ircService = new IRCService(this);
        }
    }
}
