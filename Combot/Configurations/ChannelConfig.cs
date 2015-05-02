using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combot.Configurations
{
    public class ChannelConfig
    {
        public string Name { get; set; }
        public string Key { get; set; }

        public ChannelConfig()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Name = string.Empty;
            Key = string.Empty;
        }
    }
}
