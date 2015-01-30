using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Combot.Modules;

namespace Combot.Configurations
{
    public class ServerConfig
    {
        public event Action ModifyEvent;
        public string Name { get; set; }
        public bool AutoConnect { get; set; }
        public string CommandPrefix { get; set; }
        public string Nickname { get; set; }
        public string Realname { get; set; }
        public string Username { get; set; }
        public List<string> Owners { get; set; } 
        public List<string> ChannelBlacklist { get; set; }
        public List<string> NickBlacklist { get; set; }
        public List<HostConfig> Hosts { get; set; }
        public List<ChannelConfig> Channels { get; set; }
        public List<Module> Modules { get; set; }

        public ServerConfig()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Name = string.Empty;
            AutoConnect = false;
            CommandPrefix = string.Empty;
            Owners = new List<string>();
            ChannelBlacklist = new List<string>();
            NickBlacklist = new List<string>();
            Channels = new List<ChannelConfig>();
            Modules = new List<Module>();
            Hosts = new List<HostConfig>();
            Nickname = string.Empty;
            Realname = string.Empty;
            Username = string.Empty;
        }

        public void Save()
        {
            if (ModifyEvent != null)
            {
                ModifyEvent();
            }
        }
    }
}
