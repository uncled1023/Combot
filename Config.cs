using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Combot
{
    class Config
    {
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (value != _name)
                {
                    _name = value;
                }
            }
        }

        private IPEndPoint _host;
        public IPEndPoint Host
        {
            get
            {
                return _host;
            }

            set
            {
                if (value != _host)
                {
                    _host = value;
                }
            }
        }

        private Nick _nick;
        public Nick Nick
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
