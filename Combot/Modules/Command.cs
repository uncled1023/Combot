using System.Collections.Generic;

namespace Combot.Modules
{
    public class Command
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public List<string> ChannelBlacklist { get; set; }
        public List<string> NickBlacklist { get; set; }
        public List<string> Triggers { get; set; }
        public List<CommandArgument> Arguments { get; set; }
        public List<AccessType> AllowedAccess { get; set; }
        public bool ShowHelp { get; set; }
        public bool SpamCheck { get; set; }

        public bool ShouldSerializeValue()
        {
            return false;
        }

        public Command()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Name = string.Empty;
            Description = string.Empty;
            Enabled = true;
            Triggers = new List<string>();
            ChannelBlacklist = new List<string>();
            NickBlacklist = new List<string>();
            Arguments = new List<CommandArgument>();
            AllowedAccess = new List<AccessType>();
            ShowHelp = true;
            SpamCheck = true;
        }

        public void Copy(Command command)
        {
            Name = command.Name;
            Description = command.Description;
            Enabled = command.Enabled;
            Triggers = new List<string>();
            foreach (string trigger in command.Triggers)
            {
                Triggers.Add(trigger);
            }
            ChannelBlacklist = new List<string>();
            foreach (string channel in command.ChannelBlacklist)
            {
                ChannelBlacklist.Add(channel);
            }
            NickBlacklist = new List<string>();
            foreach (string nick in command.NickBlacklist)
            {
                NickBlacklist.Add(nick);
            }
            Arguments = new List<CommandArgument>();
            foreach (CommandArgument arg in command.Arguments)
            {
                CommandArgument newArg = new CommandArgument();
                newArg.Copy(arg);
                Arguments.Add(newArg);
            }
            AllowedAccess = new List<AccessType>();
            foreach (AccessType accessType in command.AllowedAccess)
            {
                AllowedAccess.Add(accessType);
            }
            ShowHelp = command.ShowHelp;
            SpamCheck = command.SpamCheck;
        }
    }
}
