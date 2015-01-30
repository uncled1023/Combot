using System;
using System.Collections.Generic;
using Combot.IRCServices;

namespace Combot.Modules
{
    public class CommandMessage
    {
        public string Location { get; set; }
        public LocationType LocationType { get; set; }
        public Nick Nick { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Command { get; set; }
        public List<string> Arguments { get; set; }

        public CommandMessage()
        {
            Location = string.Empty;
            LocationType = LocationType.Channel;
            Nick = new Nick();
            TimeStamp = DateTime.Now;
            Command = string.Empty;
            Arguments = new List<string>();
        }
    }

    public enum LocationType
    {
        Channel,
        Query,
        Notice
    }
}