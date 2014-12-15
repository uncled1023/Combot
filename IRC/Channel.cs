using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combot
{
    public class Channel
    {
        public string Name { get; set; }
        public string Topic { get; set; }
        public string Key { get; set; }
        public bool AutoJoin { get; set; }
        public DateTime Registration { get; set; }
        public List<ChannelMode> Modes { get; set; }
        public List<Nick> Nicks { get; set; }

        public Channel()
        {
            Name = string.Empty;
            Topic = string.Empty;
            Key = string.Empty;
            AutoJoin = false;
            Registration = DateTime.Now;
            Modes = new List<ChannelMode>();
            Nicks = new List<Nick>();
        }

        public Channel(string name, string topic, string key, bool autojoin, DateTime registration, List<ChannelMode> modes, List<Nick> nicks)
        {
            Name = name;
            Topic = topic;
            Key = key;
            AutoJoin = autojoin;
            Registration = registration;
            Modes = modes;
            Nicks = nicks;
        }

        public void AddNick(Nick nick)
        {
            Nicks.Add(nick);
        }

        public void AddNicks(List<Nick> nicks)
        {
            Nicks.AddRange(nicks);
        }

        public void RemoveNick(Nick nick)
        {
            if (Nicks.Contains(nick))
            {
                Nicks.Remove(nick);
            }
        }

        public void RemoveNicks(List<Nick> nicks)
        {
            foreach (Nick nick in nicks)
            {
                RemoveNick(nick);
            }
        }

        public void AddMode(ChannelMode mode)
        {
            Modes.Add(mode);
        }

        public void AddModes(List<ChannelMode> modes)
        {
            Modes.AddRange(modes);
        }

        public void RemoveMode(ChannelMode mode)
        {
            if (Modes.Contains(mode))
            {
                Modes.Remove(mode);
            }
        }

        public void RemoveModes(List<ChannelMode> modes)
        {
            foreach (ChannelMode mode in modes)
            {
                RemoveMode(mode);
            }
        }
    }
}
