using System.Collections.Generic;

namespace Combot.Modules
{
    public class CommandArgument
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> Triggers { get; set; } 
        public bool Required { get; set; }

        public CommandArgument()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Name = string.Empty;
            Description = string.Empty;
            Triggers = new List<string>();
            Required = false;
        }

        public void Copy(CommandArgument argument)
        {
            Name = argument.Name;
            Description = argument.Description;
            Triggers = new List<string>();
            foreach (string trigger in argument.Triggers)
            {
                Triggers.Add(trigger);
            }
            Required = argument.Required;
        }
    }
}