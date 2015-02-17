using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using Combot.Configurations;
using Combot.Databases;
using Combot.IRCServices;
using Newtonsoft.Json;

namespace Combot.Modules
{
    public class Module
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public bool Enabled { get; set; }
        public List<string> ChannelBlacklist { get; set; }
        public List<string> NickBlacklist { get; set; }
        public List<Command> Commands { get; set; }
        public List<Option> Options { get; set; }

        public event EventHandler<string> ModuleErrorEvent;

        public bool Loaded { get; set; }
        public bool ShouldSerializeLoaded()
        {
            return false;
        }

        public string ConfigPath { get; set; }
        public bool ShouldSerializeConfigPath()
        {
            return false;
        }

        protected Bot Bot;

        private ReaderWriterLockSlim ConfigRWLock;
        private ReaderWriterLockSlim ConfigFileRWLock;
        private JsonSerializerSettings JsonSettings;

        public Module()
        {
            SetDefaults();
            ConfigRWLock = new ReaderWriterLockSlim();
            ConfigFileRWLock = new ReaderWriterLockSlim();
            JsonSettings = new JsonSerializerSettings();
            JsonSettings.Converters.Add(new IPAddressConverter());
            JsonSettings.Converters.Add(new IPEndPointConverter());
            JsonSettings.Formatting = Formatting.Indented;
        }

        public void HandleCommandEvent(CommandMessage command)
        {
            // Check to make sure the command exists, the nick or channel isn't on a blacklist, and the module is loaded.
            if (Loaded
                && Enabled
                && !ChannelBlacklist.Contains(command.Location)
                && !NickBlacklist.Contains(command.Nick.Nickname)
                && Commands.Exists(c => c.Triggers.Contains(command.Command)
                                        && c.Enabled
                                        && !c.ChannelBlacklist.Contains(command.Location)
                                        && !c.NickBlacklist.Contains(command.Nick.Nickname)
                                    )
                )
            {
                // Figure out access of the nick
                Command cmd = Commands.Find(c => c.Triggers.Contains(command.Command));
                List<AccessType> nickAccessTypes = new List<AccessType>() { AccessType.User };
                foreach (PrivilegeMode privilege in command.Nick.Privileges)
                {
                    nickAccessTypes.Add(Bot.PrivilegeModeMapping[privilege]);
                }
                if ((Bot.ServerConfig.Owners.Contains(command.Nick.Nickname) && command.Nick.Modes.Contains(UserMode.r)) || command.Nick.Nickname == Bot.IRC.Nickname)
                {
                    nickAccessTypes.Add(AccessType.Owner);
                }
                command.Access.AddRange(nickAccessTypes);
                // If they have the correct access for the command, send it
                if (cmd.AllowedAccess.Exists(access => nickAccessTypes.Contains(access)))
                {
                    ParseCommand(command);
                }
                else
                {
                    string noAccessMessage = string.Format("You do not have access to use \u0002{0}\u000F.", command.Command);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage);
                }
            }
        }

        virtual public void Initialize() { }

        virtual public void ParseCommand(CommandMessage command) { }

        protected void OnError(string e)
        {
            string errorMsg = string.Format("[{0}] {1}", Name, e);
            EventHandler<string> handler = ModuleErrorEvent;
            if (handler != null)
            {
                handler(this, errorMsg);
            }
        }

        public void SetDefaults()
        {
            Name = string.Empty;
            ClassName = string.Empty;
            Enabled = false;
            ChannelBlacklist = new List<string>();
            NickBlacklist = new List<string>();
            ConfigPath = Directory.GetCurrentDirectory();
            Loaded = false;
            Commands = new List<Command>();
            Options = new List<Option>();
        }

        public void Copy(Module module)
        {
            Name = module.Name;
            ClassName = module.ClassName;
            Enabled = module.Enabled;
            ChannelBlacklist = new List<string>();
            foreach (string channel in module.ChannelBlacklist)
            {
                ChannelBlacklist.Add(channel);
            }
            NickBlacklist = new List<string>();
            foreach (string nick in module.NickBlacklist)
            {
                NickBlacklist.Add(nick);
            }
            Commands = new List<Command>();
            foreach (Command command in module.Commands)
            {
                Command newCommand = new Command();
                newCommand.Copy(command);
                Commands.Add(newCommand);
            }
            Options = new List<Option>();
            foreach (Option option in module.Options)
            {
                Option newOption = new Option();
                newOption.Copy(option);
                Options.Add(newOption);
            }
        }

        public Module CreateInstance(Bot bot)
        {
            Module newModule = new Module();
            if (!Loaded)
            {
                //create the class base on string
                //note : include the namespace and class name (namespace=Combot.Modules, class name=<class_name>)
                Assembly a = Assembly.LoadFrom(Path.Combine(ConfigPath, string.Format("{0}.dll", Name)));
                Type t = a.GetType("Combot.Modules.Plugins." + ClassName);

                //check to see if the class is instantiated or not
                if (t != null)
                {
                    newModule = (Module)Activator.CreateInstance(t);
                    newModule.Copy(this);
                    newModule.Loaded = true;
                    newModule.ConfigPath = ConfigPath;
                    newModule.Bot = bot;
                    newModule.Initialize();
                }
            }

            return newModule;
        }

        public object GetOptionValue(string name)
        {
            object foundValue = null;
            Option foundOption = Options.Find(opt => opt.Name == name);
            if (foundOption != null)
            {
                foundValue = foundOption.Value;
                if (foundValue == null)
                {
                    foundValue = string.Empty;
                }
            }
            return foundValue;
        }

        public void SaveConfig()
        {
            ConfigFileRWLock.EnterWriteLock();

            // Serialize Config
            ConfigRWLock.EnterReadLock();
            string configContents = JsonConvert.SerializeObject(this, JsonSettings);
            ConfigRWLock.ExitReadLock();

            // Save config to file
            string path = Path.Combine(ConfigPath, "Module.config");
            using (StreamWriter streamWriter = new StreamWriter(path, false))
            {
                streamWriter.Write(configContents);
            }

            ConfigFileRWLock.ExitWriteLock();
        }

        public void LoadConfig()
        {
            ConfigFileRWLock.EnterReadLock();
            string path = Path.Combine(ConfigPath, "Module.config");
            if (File.Exists(path))
            {
                string configContents;
                using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
                {
                    configContents = streamReader.ReadToEnd();
                }

                // Load the deserialized file into the config
                ConfigRWLock.EnterWriteLock();
                Module newModule = JsonConvert.DeserializeObject<Module>(configContents, JsonSettings);
                Copy(newModule);
                ConfigRWLock.ExitWriteLock();
            }
            ConfigFileRWLock.ExitReadLock();
        }

        public void AddServer()
        {
            string search = "SELECT * FROM `servers` WHERE " +
                            "`name` = {0}";
            List<Dictionary<string, object>> results = Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name });

            if (!results.Any())
            {
                string query = "INSERT INTO `servers` SET " +
                               "`name` = {0}";
                Bot.Database.Execute(query, new object[] { Bot.ServerConfig.Name });
            }
        }

        public void AddChannel(string channel)
        {
            string search = "SELECT * FROM `channels` WHERE " +
                            "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}) AND " +
                            "`name` = {1}";
            List<Dictionary<string, object>> results = Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, channel });

            if (!results.Any())
            {
                string query = "INSERT INTO `channels` SET " +
                               "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                               "`name` = {1}";
                Bot.Database.Execute(query, new object[] { Bot.ServerConfig.Name, channel });
            }
        }

        public void AddNick(string nickname)
        {
            string search = "SELECT * FROM `nicks` WHERE " +
                            "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}) AND " +
                            "`nickname` = {1}";
            List<Dictionary<string, object>> results = Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, nickname });

            if (!results.Any())
            {
                string insert = "INSERT INTO `nicks` SET " +
                                "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                                "`nickname` = {1}";
                Bot.Database.Execute(insert, new object[] { Bot.ServerConfig.Name, nickname });
            }
        }

        public string GetNickname(int id)
        {
            string search = "SELECT `nickname` FROM `nicks` " +
                            "WHERE `id` = {0}";
            List<Dictionary<string, object>> results = Bot.Database.Query(search, new object[] { id });
            string nickname = string.Empty;
            if (results.Any())
            {
                nickname = results.First()["nickname"].ToString();
            }
            return nickname;
        }

        public void SendResponse(MessageType messageType, string location, string nickname, string message)
        {
            switch (messageType)
            {
                case MessageType.Channel:
                    Bot.IRC.Command.SendPrivateMessage(location, message);
                    break;
                case MessageType.Query:
                    Bot.IRC.Command.SendPrivateMessage(nickname, message);
                    break;
                case MessageType.Notice:
                    Bot.IRC.Command.SendNotice(nickname, message);
                    break;
            }
        }
    }
}
