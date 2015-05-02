using System;

namespace Combot.Modules.Plugins
{
    public class SpamHighlightInfo
    {
        public string Channel { get; set; }
        public string Nick { get; set; }
        public int Highlights { get; set; }
        public DateTime FirstHighlightTime { get; set; }

        public SpamHighlightInfo()
        {
            SetDefaults();
        }

        private void SetDefaults()
        {
            Channel = string.Empty;
            Nick = string.Empty;
            Highlights = 0;
            FirstHighlightTime = DateTime.Now;
        }
    }
}