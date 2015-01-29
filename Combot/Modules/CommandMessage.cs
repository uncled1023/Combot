using System;
using System.Collections.Generic;
using Combot.IRCServices;

namespace Combot.Modules
{
    public class CommandMessage
    {
        public Nick Nick { get; set; }
        public DateTime TimeStamp { get; set; }
        public string ModuleName { get; set; }
        public string Command { get; set; }
        public List<string> Arguments { get; set; }

        public CommandMessage()
        {
            Nick = new Nick();
            TimeStamp = DateTime.Now;
            ModuleName = string.Empty;
            Command = string.Empty;
            Arguments = new List<string>();
        }
    }
}