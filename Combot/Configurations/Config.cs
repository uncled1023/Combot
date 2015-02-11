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
                config.LoadEvent += LoadServers;
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
            string ConfigPath = Path.Combine(Directory.GetCurrentDirectory(), @"Combot.Servers.config");
            using (StreamWriter streamWriter = new StreamWriter(ConfigPath, false))
            {
                streamWriter.Write(configContents);
            }

            ConfigFileRWLock.ExitWriteLock();
        }

        public void LoadServers()
        {
            ConfigFileRWLock.EnterReadLock();
            string ConfigPath = Path.Combine(Directory.GetCurrentDirectory(), @"Combot.Servers.config");
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
                }
                ConfigRWLock.ExitWriteLock();
            }
            ConfigFileRWLock.ExitReadLock();
        }

        public void UpdateServers()
        {
            ConfigFileRWLock.EnterReadLock();
            string ConfigPath = Path.Combine(Directory.GetCurrentDirectory(), @"Combot.Servers.config");
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

                for (int i = 0; i < newConfigs.Count; i++)
                {
                    if (Servers.Count > i)
                    {
                        Servers[i].Copy(newConfigs[i]);
                    }
                    else
                    {
                        Servers.Add(newConfigs[i]);
                    }
                }
                ConfigRWLock.ExitWriteLock();
            }
            ConfigFileRWLock.ExitReadLock();
        }
    }
}
