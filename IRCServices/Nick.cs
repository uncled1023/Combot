using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combot.IRCServices
{
    public class Nick
    {
        public string Username { get; set; }
        public string Realname { get; set; }
        public string Host { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }
        public bool Identified { get; set; }
        public bool Registered { get; set; }
        public List<UserMode> Modes { get; set; }
        public List<PrivaledgeMode> Privaledges { get; set; }

        public Nick()
        {
            Username = string.Empty;
            Realname = string.Empty;
            Host = string.Empty;
            Nickname = string.Empty;
            Password = string.Empty;
            Identified = false;
            Registered = false;
            Modes = new List<UserMode>();
            Privaledges = new List<PrivaledgeMode>();
        }

        public void AddMode(UserMode mode)
        {
            if (!Modes.Contains(mode))
            {
                Modes.Add(mode);
            }
        }

        public void AddModes(List<UserMode> modes)
        {
            foreach (UserMode mode in modes)
            {
                AddMode(mode);
            }
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

        public void AddPrivaledge(PrivaledgeMode privaledge)
        {
            if (!Privaledges.Contains(privaledge))
            {
                Privaledges.Add(privaledge);
            }
        }

        public void AddPrivaledges(List<PrivaledgeMode> privaledges)
        {
            foreach (PrivaledgeMode privaledge in privaledges)
            {
                AddPrivaledge(privaledge);
            }
        }

        public void RemovePrivaledge(PrivaledgeMode privaledge)
        {
            if (Privaledges.Contains(privaledge))
            {
                Privaledges.Remove(privaledge);
            }
        }

        public void RemovePrivaledges(List<PrivaledgeMode> privaledges)
        {
            foreach (PrivaledgeMode privaledge in privaledges)
            {
                RemovePrivaledge(privaledge);
            }
        }
    }
}
