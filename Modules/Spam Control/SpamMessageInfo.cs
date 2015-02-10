using System;

namespace Combot.Modules.Plugins
{
    public class SpamMessageInfo
    {
        public string Channel { get; set; }
        public string Nick { get; set; }
        public int Lines { get; set; }
        public DateTime FirstMessageTime { get; set; }

        public SpamMessageInfo()
        {
            SetDefaults();
        }

        private void SetDefaults()
        {
            Channel = string.Empty;
            Nick = string.Empty;
            Lines = 0;
            FirstMessageTime = DateTime.Now;
        }
    }
}