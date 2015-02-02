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
        public List<string> Nicknames { get; set; }
        public string Realname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public List<string> Owners { get; set; } 
        public List<string> ChannelBlacklist { get; set; }
        public List<string> NickBlacklist { get; set; }
        public List<HostConfig> Hosts { get; set; }
        public List<ChannelConfig> Channels { get; set; }
        public List<Module> Modules { get; set; }
        public bool AutoConnect { get; set; }
        public bool AutoRegister { get; set; }
        public string CommandPrefix { get; set; }
        public int JoinDelay { get; set; }
        public int MaxMessageLength { get; set; }
        public int MessageSendDelay { get; set; }

        public ServerConfig()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Name = string.Empty;
            Nicknames = new List<string>();
            Realname = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
            Email = string.Empty;
            AutoConnect = false;
            AutoRegister = false;
            CommandPrefix = string.Empty;
            JoinDelay = 0;
            MaxMessageLength = 400;
            MessageSendDelay = 0;
            Owners = new List<string>();
            ChannelBlacklist = new List<string>();
            NickBlacklist = new List<string>();
            Channels = new List<ChannelConfig>();
            Modules = new List<Module>();
            Hosts = new List<HostConfig>();
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
