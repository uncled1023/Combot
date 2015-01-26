using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combot.Configurations
{
    public class HostConfig
    {
        public HostConfig()
        {
            Host = string.Empty;
            Port = 0;
        }

        private string _Host;
        public string Host
        {
            get
            {
                return _Host;
            }

            set
            {
                if (_Host != value)
                {
                    _Host = value;
                }
            }
        }

        private int _Port;
        public int Port
        {
            get
            {
                return _Port;
            }

            set
            {
                if (_Port != value)
                {
                    _Port = value;
                }
            }
        }
    }
}
