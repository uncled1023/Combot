using System;
using System.Collections.Generic;

namespace Combot.Configurations
{
    public class ServerConfig
    {
        public event Action ModifyEvent;
        public event Action LoadEvent;
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
        public DatabaseConfig Database { get; set; }
        public string ModuleLocation { get; set; }
        public bool AutoConnect { get; set; }
        public bool Reconnect { get; set; }
        public bool AutoRegister { get; set; }
        public string CommandPrefix { get; set; }
        public int JoinDelay { get; set; }
        public int MaxMessageLength { get; set; }
        public int MessageSendDelay { get; set; }
        public SpamSourceType SpamSourceType { get; set; }
        public int SpamCountMax { get; set; }
        public TimeSpan SpamSessionTime { get; set; }

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
            SpamSourceType = SpamSourceType.Nick;
            SpamCountMax = 5;
            SpamSessionTime = new TimeSpan(0, 0, 1);
            ModuleLocation = string.Empty;
            Owners = new List<string>();
            ChannelBlacklist = new List<string>();
            NickBlacklist = new List<string>();
            Channels = new List<ChannelConfig>();
            Hosts = new List<HostConfig>();
            Database = new DatabaseConfig();
        }

        public void Copy(ServerConfig config)
        {
            Name = config.Name;
            Nicknames = new List<string>();
            for (int i = 0; i < config.Nicknames.Count; i++)
            {
                Nicknames.Add(config.Nicknames[i]);
            }
            Realname = config.Realname;
            Username = config.Username;
            Password = config.Password;
            Email = config.Email;
            AutoConnect = config.AutoConnect;
            AutoRegister = config.AutoRegister;
            CommandPrefix = config.CommandPrefix;
            JoinDelay = config.JoinDelay;
            MaxMessageLength = config.MaxMessageLength;
            MessageSendDelay = config.MessageSendDelay;
            SpamSourceType = config.SpamSourceType;
            SpamCountMax = config.SpamCountMax;
            SpamSessionTime = config.SpamSessionTime;
            ModuleLocation = config.ModuleLocation;
            Owners = new List<string>();
            for (int i = 0; i < config.Owners.Count; i++)
            {
                Owners.Add(config.Owners[i]);
            }
            ChannelBlacklist = new List<string>();
            for (int i = 0; i < config.ChannelBlacklist.Count; i++)
            {
                ChannelBlacklist.Add(config.ChannelBlacklist[i]);
            }
            NickBlacklist = new List<string>();
            for (int i = 0; i < config.NickBlacklist.Count; i++)
            {
                NickBlacklist.Add(config.NickBlacklist[i]);
            }
            Channels = new List<ChannelConfig>();
            for (int i = 0; i < config.Channels.Count; i++)
            {
                Channels.Add(config.Channels[i]);
            }
            Hosts = new List<HostConfig>();
            for (int i = 0; i < config.Hosts.Count; i++)
            {
                Hosts.Add(config.Hosts[i]);
            }
            Database = config.Database;
        }

        public void Save()
        {
            if (ModifyEvent != null)
            {
                ModifyEvent();
            }
        }

        public void Load()
        {
            if (LoadEvent != null)
            {
                LoadEvent();
            }
        }
    }
}
