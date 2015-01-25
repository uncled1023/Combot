using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Combot
{
    public class Config
    {
        private Server _server;
        public Server Server
        {
            get
            {
                return _server;
            }

            set
            {
                if (value != _server)
                {
                    _server = value;
                }
            }
        }

        private string _realname;
        public string Realname
        {
            get
            {
                return _realname;
            }

            set
            {
                if (value != _realname)
                {
                    _realname = value;
                }
            }
        }

        private string _nick;
        public string Nick
        {
            get
            {
                return _nick;
            }

            set
            {
                if (value != _nick)
                {
                    _nick = value;
                }
            }
        }
    }
}
