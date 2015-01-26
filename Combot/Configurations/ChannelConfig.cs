using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combot.Configurations
{
    public class ChannelConfig
    {
        public ChannelConfig()
        {
            Name = string.Empty;
            Key = string.Empty;
        }

        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }

            set
            {
                if (_Name != value)
                {
                    _Name = value;
                }
            }
        }

        private string _Key;
        public string Key
        {
            get
            {
                return _Key;
            }

            set
            {
                if (_Key != value)
                {
                    _Key = value;
                }
            }
        }
    }
}
