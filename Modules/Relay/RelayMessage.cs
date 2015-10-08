using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combot.Modules.Plugins
{
    public class RelayMessage
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public RelayType Type { get; set; }
        public string Message { get; set; }

        public RelayMessage()
        {
            SetDefaults();
        }

        private void SetDefaults()
        {
            Source = string.Empty;
            Target = string.Empty;
            Type = RelayType.Message;
            Message = string.Empty;
        }
    }
}
