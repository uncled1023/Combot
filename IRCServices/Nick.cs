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
        public List<UserMode> Modes { get; set; }
        public List<PrivilegeMode> Privileges { get; set; }

        public Nick()
        {
            Username = string.Empty;
            Realname = string.Empty;
            Host = string.Empty;
            Nickname = string.Empty;
            Password = string.Empty;
            Modes = new List<UserMode>();
            Privileges = new List<PrivilegeMode>();
        }

        public void Copy(Nick nick)
        {
            Username = nick.Username;
            Realname = nick.Realname;
            Host = nick.Host;
            Nickname = nick.Nickname;
            Password = nick.Password;
            Modes = new List<UserMode>();
            Modes.AddRange(nick.Modes);
            Privileges = new List<PrivilegeMode>();
            Privileges.AddRange(nick.Privileges);
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

        public void AddPrivilege(PrivilegeMode Privilege)
        {
            if (!Privileges.Contains(Privilege))
            {
                Privileges.Add(Privilege);
            }
        }

        public void AddPrivileges(List<PrivilegeMode> Privileges)
        {
            foreach (PrivilegeMode Privilege in Privileges)
            {
                AddPrivilege(Privilege);
            }
        }

        public void RemovePrivilege(PrivilegeMode Privilege)
        {
            if (Privileges.Contains(Privilege))
            {
                Privileges.Remove(Privilege);
            }
        }

        public void RemovePrivileges(List<PrivilegeMode> Privileges)
        {
            foreach (PrivilegeMode Privilege in Privileges)
            {
                RemovePrivilege(Privilege);
            }
        }
    }
}
