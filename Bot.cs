using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combot
{
    class Bot
    {
        public Config Config;

        private IRCService _ircService;

        internal Bot()
        {
            _ircService = new IRCService(this);
        }
    }
}
