using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Combot.IRCServices;

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

        public bool Loaded { get; set; }
        public bool ShouldSerializeLoaded()
        {
            return false;
        }

        protected Bot Bot;

        public Module()
        {
            SetDefaults();
        }

        public void HandleCommandEvent(CommandMessage command)
        {
            // Check to make sure the command exists, the nick or channel isn't on a blacklist, and the module is loaded.
            if (Loaded
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
                if (Bot.ServerConfig.Owners.Contains(command.Nick.Nickname) && command.Nick.Modes.Contains(UserMode.r))
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
                    switch (command.MessageType)
                    {
                        case MessageType.Channel:
                            Bot.IRC.SendPrivateMessage(command.Location, noAccessMessage);
                            break;
                        case MessageType.Query:
                            Bot.IRC.SendPrivateMessage(command.Nick.Nickname, noAccessMessage);
                            break;
                        case MessageType.Notice:
                            Bot.IRC.SendNotice(command.Nick.Nickname, noAccessMessage);
                            break;
                    }
                }
            }
        }

        virtual public void Initialize() { }

        virtual public void ParseCommand(CommandMessage command) { }

        public void SetDefaults()
        {
            Name = string.Empty;
            ClassName = string.Empty;
            Enabled = false;
            ChannelBlacklist = new List<string>();
            NickBlacklist = new List<string>();
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
                //note : include the namespace and class name (namespace=Bot.Modules, class name=<class_name>)
                Assembly a = Assembly.Load("Combot");
                Type t = a.GetType("Combot.Modules.ModuleClasses." + ClassName);

                //check to see if the class is instantiated or not
                if (t != null)
                {
                    newModule = (Module)Activator.CreateInstance(t);
                    newModule.Copy(this);
                    newModule.Loaded = true;
                    newModule.Bot = bot;
                    newModule.Initialize();
                }
            }

            return newModule;
        }

        public dynamic GetOptionValue(string name)
        {
            dynamic foundValue = null;
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
    }
}
