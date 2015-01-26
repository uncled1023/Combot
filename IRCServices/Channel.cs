using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combot.IRCServices
{
    public class Channel
    {
        public string Name { get; set; }
        public string Topic { get; set; }
        public string Key { get; set; }
        public bool AutoJoin { get; set; }
        public bool Joined { get; set; }
        public DateTime Registration { get; set; }
        public List<string> Bans { get; set; }
        public List<ChannelMode> Modes { get; set; }
        public List<Nick> Nicks { get; set; }

        public Channel()
        {
            Name = string.Empty;
            Topic = string.Empty;
            Key = string.Empty;
            AutoJoin = false;
            Joined = false;
            Registration = DateTime.Now;
            Bans = new List<string>();
            Modes = new List<ChannelMode>();
            Nicks = new List<Nick>();
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

        public void RemoveNick(string nickname)
        {
            if (Nicks.Exists(nick => nick.Nickname == nickname))
            {
                Nicks.Remove(Nicks.Find(nick => nick.Nickname == nickname));
            }
        }

        public void RemoveNicks(List<Nick> nicks)
        {
            foreach (Nick nick in nicks)
            {
                RemoveNick(nick);
            }
        }

        public void RemoveNicks(List<string> nicks)
        {
            foreach (string nick in nicks)
            {
                RemoveNick(nick);
            }
        }

        public Nick GetNick(string nickname)
        {
            Nick foundNick = Nicks.Find(nick => nick.Nickname == nickname);
            return foundNick;
        }

        public List<Nick> GetNicks(List<string> nicknames)
        {
            List<Nick> foundNicks = new List<Nick>();
            foreach (string nickname in nicknames)
            {
                Nick foundNick = GetNick(nickname);
                if (foundNick != null)
                {
                    foundNicks.Add(foundNick);
                }
            }
            return foundNicks;
        }

        public void AddMode(ChannelMode mode)
        {
            if (!Modes.Contains(mode))
            {
                Modes.Add(mode);
            }
        }

        public void AddModes(List<ChannelMode> modes)
        {
            foreach (ChannelMode mode in modes)
            {
                AddMode(mode);
            }
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

        public void AddBan(string mask)
        {
            if (!Bans.Contains(mask))
            {
                Bans.Add(mask);
            }
        }

        public void RemoveBan(string mask)
        {
            Bans.Remove(mask);
        }
    }
}
