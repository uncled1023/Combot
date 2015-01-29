using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Combot.Modules
{
    public class Module
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public bool Enabled { get; set; }
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

        virtual public void Initialize() { }

        virtual public void HandleCommandEvent(CommandMessage command) { }

        public void SetDefaults()
        {
            Name = string.Empty;
            ClassName = string.Empty;
            Enabled = false;
            Loaded = false;
            Commands = new List<Command>();
            Options = new List<Option>();
        }

        public void Copy(Module module)
        {
            Name = module.Name;
            ClassName = module.ClassName;
            Enabled = module.Enabled;
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
    }
}
