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
        public List<MessageType> AllowedMessageTypes { get; set; } 
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
            AllowedMessageTypes = new List<MessageType>();
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
            AllowedMessageTypes = new List<MessageType>();
            foreach (MessageType messageType in command.AllowedMessageTypes)
            {
                AllowedMessageTypes.Add(messageType);
            }
            AllowedAccess = new List<AccessType>();
            foreach (AccessType accessType in command.AllowedAccess)
            {
                AllowedAccess.Add(accessType);
            }
            ShowHelp = command.ShowHelp;
            SpamCheck = command.SpamCheck;
        }

        public List<CommandArgument> GetValidArguments(List<string> passedArgs, MessageType messageType)
        {
            List<CommandArgument> validArguments = new List<CommandArgument>();
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (Arguments[i].MessageTypes.Contains(messageType))
                {
                    if (Arguments[i].DependentArguments.Count > 0)
                    {
                        if (Arguments[i].DependentArguments.Exists(arg => Arguments.Exists(val => val.Name == arg.Name)))
                        {
                            CommandArgument.DependentArgumentInfo checkedArgument = Arguments[i].DependentArguments.Find(dep => Arguments.Exists(val => val.Name == dep.Name));
                            int argIndex = validArguments.FindIndex(arg => arg.Name == checkedArgument.Name);
                            if (passedArgs.Count > argIndex)
                            {
                                if (checkedArgument.Values.Exists(check => check.ToLower() == passedArgs[argIndex].ToLower()))
                                {
                                    CommandArgument newArgument = new CommandArgument();
                                    newArgument.Copy(Arguments[i]);
                                    validArguments.Add(newArgument);
                                }
                            }
                            else
                            {
                                CommandArgument newArgument = new CommandArgument();
                                newArgument.Copy(Arguments[i]);
                                validArguments.Add(newArgument);
                            }
                        }
                    }
                    else
                    {
                        CommandArgument newArgument = new CommandArgument();
                        newArgument.Copy(Arguments[i]);
                        validArguments.Add(newArgument);
                    }
                }
            }
            return validArguments;
        }
    }
}
