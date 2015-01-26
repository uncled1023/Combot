using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace Combot.Configurations
{
    public class ServerConfig
    {
        public event Action ModifyEvent;

        public ServerConfig()
        {
            AutoConnect = false;
            Channels = new List<ChannelConfig>();
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

        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }

            set
            {
                if (_Name != value)
                {
                    _Name = value;
                }
            }
        }

        private string _Nickname;
        public string Nickname 
        { 
            get
            {
                return _Nickname;
            }

            set
            {
                if (_Nickname != value)
                {
                    _Nickname = value;
                }
            }
        }

        private string _Username;
        public string Username
        {
            get
            {
                return _Username;
            }

            set
            {
                if (_Username != value)
                {
                    _Username = value;
                }
            }
        }

        private string _Realname;
        public string Realname
        {
            get
            {
                return _Realname;
            }

            set
            {
                if (_Realname != value)
                {
                    _Realname = value;
                }
            }
        }

        private List<HostConfig> _Hosts;
        public List<HostConfig> Hosts
        {
            get
            {
                return _Hosts;
            }

            set
            {
                if (_Hosts != value)
                {
                    _Hosts = value;
                }
            }
        }

        private List<ChannelConfig> _Channels;
        public List<ChannelConfig> Channels
        {
            get
            {
                return _Channels;
            }

            set
            {
                if (_Channels != value)
                {
                    _Channels = value;
                }
            }
        }

        private bool _AutoConnect;
        public bool AutoConnect
        {
            get
            {
                return _AutoConnect;
            }

            set
            {
                if (_AutoConnect != value)
                {
                    _AutoConnect = value;
                }
            }
        }
    }
}
