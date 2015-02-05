using System.Collections.Generic;

namespace Combot.Modules
{
    public class CommandArgument
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> AllowedValues { get; set; }
        public List<DependentArgumentInfo> DependentArguments { get; set; } 
        public List<MessageType> MessageTypes { get; set; }
        public bool Required { get; set; }

        public CommandArgument()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Name = string.Empty;
            Description = string.Empty;
            AllowedValues = new List<string>();
            DependentArguments = new List<DependentArgumentInfo>();
            MessageTypes = new List<MessageType>();
            Required = false;
        }

        public void Copy(CommandArgument argument)
        {
            Name = argument.Name;
            Description = argument.Description;
            AllowedValues = new List<string>();
            foreach (string value in argument.AllowedValues)
            {
                AllowedValues.Add(value);
            }
            DependentArguments = new List<DependentArgumentInfo>();
            foreach (DependentArgumentInfo value in argument.DependentArguments)
            {
                DependentArguments.Add(value);
            }
            MessageTypes = new List<MessageType>();
            foreach (MessageType value in argument.MessageTypes)
            {
                MessageTypes.Add(value);
            }
            Required = argument.Required;
        }

        public class DependentArgumentInfo
        {
            public string Name { get; set; }
            public List<string> Values { get; set; }

            public DependentArgumentInfo()
            {
                SetDefaults();
            }

            public void SetDefaults()
            {
                Name = string.Empty;
                Values = new List<string>();
            }
        }
    }
}