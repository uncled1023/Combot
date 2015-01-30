using System.Collections.Generic;

namespace Combot.Modules
{
    public class CommandArgument
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }

        public CommandArgument()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Name = string.Empty;
            Description = string.Empty;
            Required = false;
        }

        public void Copy(CommandArgument argument)
        {
            Name = argument.Name;
            Description = argument.Description;
            Required = argument.Required;
        }
    }
}