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
        public List<ChannelMode> Modes { get; set; }
        public List<Nick> Nicks { get; set; }

        private IRC _IRC;

        public Channel(IRC irc)
        {
            Name = string.Empty;
            Topic = string.Empty;
            Key = string.Empty;
            AutoJoin = false;
            Joined = false;
            Registration = DateTime.Now;
            Modes = new List<ChannelMode>();
            Nicks = new List<Nick>();
            _IRC = irc;
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
            if (!Modes.Contains(mode))
            {
                ChannelModeInfo modeInfo = new ChannelModeInfo();
                modeInfo.Mode = mode;
                modeInfo.Set = true;
                _IRC.IRCSendMode(Name, modeInfo);
                Modes.Add(mode);
            }
        }

        public void AddModes(List<ChannelMode> modes)
        {
            if (!modes.TrueForAll(mode => Modes.Contains(mode)))
            {
                List<ChannelModeInfo> modeInfos = new List<ChannelModeInfo>();
                modes.ForEach(mode => modeInfos.Add(new ChannelModeInfo() { Mode = mode, Set = true }));
                _IRC.IRCSendMode(Name, modeInfos);
                Modes.AddRange(modes);
            }
        }

        public void RemoveMode(ChannelMode mode)
        {
            if (Modes.Contains(mode))
            {
                ChannelModeInfo modeInfo = new ChannelModeInfo();
                modeInfo.Mode = mode;
                modeInfo.Set = false;
                _IRC.IRCSendMode(Name, modeInfo);
                Modes.Remove(mode);
            }
        }

        public void RemoveModes(List<ChannelMode> modes)
        {
            List<ChannelMode> validModes = Modes.FindAll(mode => mode == modes.Find(m => m == mode));
            List<ChannelModeInfo> modeInfos = new List<ChannelModeInfo>();
            validModes.ForEach(mode => modeInfos.Add(new ChannelModeInfo() { Mode = mode, Set = true }));
            validModes.ForEach(mode => Modes.Remove(mode));
            _IRC.IRCSendMode(Name, modeInfos);
        }

        public void Join()
        {
            if (!Joined)
            {
                _IRC.IRCSendJoin(Name, Key);
                Joined = true;
            }
        }

        public void Part()
        {
            if (Joined)
            {
                _IRC.IRCSendPart(Name);
                Joined = false;
            }
        }

        public void SetTopic(string topic)
        {

            if (Joined)
            {
                _IRC.IRCSendTopic(Name, topic);
            }
        }

        public string GetTopic()
        {

            return string.Empty;
        }
    }
}
