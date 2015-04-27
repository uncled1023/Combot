using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace Combot.Configurations
{
    public class Config
    {
        private ReaderWriterLockSlim ConfigRWLock;
        private ReaderWriterLockSlim ConfigFileRWLock;
        private JsonSerializerSettings JsonSettings;

        public Config()
        {
            ConfigRWLock = new ReaderWriterLockSlim();
            ConfigFileRWLock = new ReaderWriterLockSlim();
            Servers = new List<ServerConfig>();
            JsonSettings = new JsonSerializerSettings();
            JsonSettings.Converters.Add(new IPAddressConverter());
            JsonSettings.Converters.Add(new IPEndPointConverter());
            JsonSettings.Formatting = Formatting.Indented;
        }

        private List<ServerConfig> _servers;
        public List<ServerConfig> Servers
        {
            get
            {
                return _servers;
            }

            private set
            {
                if (value != _servers)
                {
                    _servers = value;
                }
            }
        }

        public void AddServer(ServerConfig config)
        {
            ConfigRWLock.EnterWriteLock();
            if (!Servers.Exists(server => server.Name == config.Name))
            {
                config.ModifyEvent += SaveServers;
                config.LoadEvent += UpdateServers;
                Servers.Add(config);
            }
            ConfigRWLock.ExitWriteLock();
        }

        public void SaveServers()
        {
            ConfigFileRWLock.EnterWriteLock();

            // Serialize Config
            ConfigRWLock.EnterReadLock();
            string configContents = JsonConvert.SerializeObject(Servers, JsonSettings);
            ConfigRWLock.ExitReadLock();

            // Save config to file
            string ConfigPath = Path.Combine(Directory.GetCurrentDirectory(), @"Combot.Servers.json");
            using (StreamWriter streamWriter = new StreamWriter(ConfigPath, false))
            {
                streamWriter.Write(configContents);
            }

            ConfigFileRWLock.ExitWriteLock();
        }

        public void LoadServers()
        {
            ConfigFileRWLock.EnterReadLock();
            string ConfigPath = Path.Combine(Directory.GetCurrentDirectory(), @"Combot.Servers.json");

            if (!File.Exists(ConfigPath))
            {
                string defaultPath = Path.Combine(Directory.GetCurrentDirectory(), @"Combot.Servers.Default.json");
                if (File.Exists(defaultPath))
                {
                    File.Copy(defaultPath, ConfigPath);
                }
            }

            if (File.Exists(ConfigPath))
            {
                string configContents;
                using (StreamReader streamReader = new StreamReader(ConfigPath, Encoding.UTF8))
                {
                    configContents = streamReader.ReadToEnd();
                }

                // Load the deserialized file into the config
                ConfigRWLock.EnterWriteLock();
                Servers = JsonConvert.DeserializeObject<List<ServerConfig>>(configContents, JsonSettings);

                for (int i = 0; i < Servers.Count; i++)
                {
                    Servers[i].ModifyEvent += SaveServers;
                    Servers[i].LoadEvent += UpdateServers;
                }
                ConfigRWLock.ExitWriteLock();
            }
            ConfigFileRWLock.ExitReadLock();
        }

        public void UpdateServers()
        {
            ConfigFileRWLock.EnterReadLock();
            string ConfigPath = Path.Combine(Directory.GetCurrentDirectory(), @"Combot.Servers.json");

            if (!File.Exists(ConfigPath))
            {
                string defaultPath = Path.Combine(Directory.GetCurrentDirectory(), @"Combot.Servers.Default.json");
                if (File.Exists(defaultPath))
                {
                    File.Copy(defaultPath, ConfigPath);
                }
            }

            if (File.Exists(ConfigPath))
            {
                string configContents;
                using (StreamReader streamReader = new StreamReader(ConfigPath, Encoding.UTF8))
                {
                    configContents = streamReader.ReadToEnd();
                }

                // Load the deserialized file into the config
                ConfigRWLock.EnterWriteLock();
                List<ServerConfig> newConfigs = JsonConvert.DeserializeObject<List<ServerConfig>>(configContents, JsonSettings);

                foreach (ServerConfig newConfig in newConfigs)
                {
                    if (Servers.Exists(server => server.Name == newConfig.Name))
                    {
                        Servers.Find(server => server.Name == newConfig.Name).Copy(newConfig);
                    }
                    else
                    {
                        Servers.Add(newConfig);
                    }
                }
                ConfigRWLock.ExitWriteLock();
            }
            ConfigFileRWLock.ExitReadLock();
        }
    }
}
