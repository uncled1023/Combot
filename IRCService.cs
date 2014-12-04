using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combot
{
    internal class IRCService
    {
        private TCPInterface _tcp;

        internal IRCService()
        {
            _tcp = new TCPInterface();
        }

        
    }
}
