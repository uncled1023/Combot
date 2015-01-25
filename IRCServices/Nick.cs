using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combot.IRCServices
{
    public class Nick
    {
        public string Realname { get; set; }
        public string Host { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }
        public bool Identified { get; set; }
        public bool Registered { get; set; }
        public List<UserMode> Modes { get; set; }

        public Nick()
        {
            Realname = string.Empty;
            Host = string.Empty;
            Nickname = string.Empty;
            Password = string.Empty;
            Identified = false;
            Registered = false;
            Modes = new List<UserMode>();
        }

        public Nick(string realname, string host, string nickname, string password, bool identified, bool registered, List<UserMode> modes)
        {
            Realname = realname;
            Host = host;
            Nickname = nickname;
            Password = password;
            Identified = identified;
            Registered = registered;
            Modes = modes;
        }

        public void AddMode(UserMode mode)
        {
            Modes.Add(mode);
        }

        public void AddModes(List<UserMode> modes)
        {
            Modes.AddRange(modes);
        }

        public void RemoveMode(UserMode mode)
        {
            if (Modes.Contains(mode))
            {
                Modes.Remove(mode);
            }
        }

        public void RemoveModes(List<UserMode> modes)
        {
            foreach (UserMode mode in modes)
            {
                RemoveMode(mode);
            }
        }
    }
}
