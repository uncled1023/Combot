using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace Combot.Modules.Plugins
{
    public class Help : Module
    {
        public override void Initialize()
        {
            Bot.CommandReceivedEvent += HandleCommandEvent;
        }

        public override void ParseCommand(CommandMessage command)
        {
            Command foundCommand = Commands.Find(c => c.Triggers.Contains(command.Command));

            if (foundCommand.Name == "Help")
            {
                if (command.Arguments.Count == 0)
                {
                    SendFullHelp(command.Nick.Nickname, command.Access);
                }
                else if (command.Arguments.ContainsKey("Command"))
                {
                    SendCommandHelp(command);
                }
            }
        }

        private void SendFullHelp(string recipient, List<AccessType> access)
        {
            Bot.IRC.Command.SendNotice(recipient, string.Format("You have the following commands available to use.  " +
                                                        "To use them either type \u0002{1}\u001Fcommand trigger\u001F\u0002 into a channel, send a private message by typing \u0002/msg {0} \u001Fcommand trigger\u001F\u0002, or send a notice by typing \u0002/notice {0} \u001Fcommand trigger\u001F\u0002.",
                                                        Bot.IRC.Nickname, Bot.ServerConfig.CommandPrefix));
            Bot.IRC.Command.SendNotice(recipient, "\u200B");
            List<string> commandList = new List<string>();
            foreach (Module module in Bot.Modules)
            {
                module.Commands.ForEach(command =>
                {
                    if (command.AllowedAccess.Exists(allowed => access.Contains(allowed)) && command.ShowHelp)
                    {
                        commandList.Add(command.Name);
                    }
                });
            }
            Bot.IRC.Command.SendNotice(recipient, string.Format("\u0002{0}\u0002", string.Join("\u0002, \u0002", commandList)));
            Bot.IRC.Command.SendNotice(recipient, "\u200B");
            Bot.IRC.Command.SendNotice(recipient, string.Format("For more information on a specific command, including viewing the triggers, type \u0002{0}help \u001Fcommand\u001F\u0002.", Bot.ServerConfig.CommandPrefix));
        }

        private void SendCommandHelp(CommandMessage command)
        {
            string helpCommand = command.Arguments["Command"].ToString();
            Module foundModule = Bot.Modules.Find(mod => mod.Commands.Exists(cmd => (cmd.Name.ToLower() == helpCommand.ToLower() || cmd.Triggers.Contains(helpCommand)) && cmd.ShowHelp));
            if (foundModule != null)
            {
                Command foundCommand = foundModule.Commands.Find(cmd => (cmd.Name.ToLower() == helpCommand.ToLower() || cmd.Triggers.Contains(helpCommand)));
                if (foundCommand != null)
                {
                    if (foundCommand.AllowedAccess.Exists(allowed => command.Access.Contains(allowed)))
                    {
                        Bot.IRC.Command.SendNotice(command.Nick.Nickname, string.Format("Help information for \u0002{0}\u0002", foundCommand.Name));
                        if (foundCommand.Description != string.Empty)
                        {
                            Bot.IRC.Command.SendNotice(command.Nick.Nickname, string.Format("{0}", foundCommand.Description));
                        }
                        Bot.IRC.Command.SendNotice(command.Nick.Nickname, "\u200B");
                        Dictionary<string, string> argDescriptions = new Dictionary<string, string>();
                        int index = 0;
                        for (int i = 0; i < foundCommand.AllowedMessageTypes.Count; i++)
                        {
                            MessageType messageType = foundCommand.AllowedMessageTypes[i];

                            // Generate Message Syntax
                            string messageSyntax = string.Empty;
                            switch (messageType)
                            {
                                case MessageType.Channel:
                                    messageSyntax = "\u0002/msg \u001Fchannel\u001F\u0002";
                                    break;
                                case MessageType.Query:
                                    messageSyntax = string.Format("\u0002/msg {0}\u0002", Bot.IRC.Nickname);
                                    break;
                                case MessageType.Notice:
                                    messageSyntax = string.Format("\u0002/notice {0}\u0002", Bot.IRC.Nickname);
                                    break;
                            }
                            List<CommandArgument> validArguments = foundCommand.Arguments.FindAll(arg => arg.MessageTypes.Contains(messageType));
                            string argHelp = string.Empty;
                            if (validArguments.Count > 0)
                            {
                                argHelp = string.Format(" \u0002{0}\u0002", string.Join(" ", validArguments.Select(arg =>
                                {
                                    string argString = string.Empty;
                                    if (arg.DependentArguments.Count > 0)
                                    {
                                        argString = "(";
                                    }
                                    if (arg.Required)
                                    {
                                        argString += "\u001F" + arg.Name + "\u001F";
                                    }
                                    else
                                    {
                                        argString += "[\u001F" + arg.Name + "\u001F]";
                                    }
                                    if (arg.DependentArguments.Count > 0)
                                    {
                                        argString += string.Format("\u0002:When {0}\u0002)", string.Join(" or ", arg.DependentArguments.Select(dep => { return string.Format("\u0002\u001F{0}\u001F\u0002=\u0002{1}\u0002", dep.Name, string.Join(",", dep.Values)); })));
                                    }
                                    return argString;
                                })));
                            }
                            if (foundCommand.Triggers.Any())
                            {
                                string triggerString = (foundCommand.Triggers.Count > 1) ? string.Format("({0})", string.Join("|", foundCommand.Triggers)) : foundCommand.Triggers.First();
                                Bot.IRC.Command.SendNotice(command.Nick.Nickname, string.Format("Syntax: {0} {1}\u0002{2}\u0002{3}", messageSyntax, Bot.ServerConfig.CommandPrefix, triggerString, argHelp));
                            }

                            // Display argument help
                            if (validArguments.Count > 0)
                            {
                                validArguments.ForEach(arg =>
                                {
                                    string commandDesc = string.Empty;
                                    if (arg.Description != string.Empty)
                                    {
                                        commandDesc = string.Format(" - {0}", arg.Description);
                                    }
                                    if (!argDescriptions.ContainsKey(arg.Name))
                                    {
                                        argDescriptions.Add(arg.Name, string.Format("\t\t\u0002{0}\u0002{1}", arg.Name, commandDesc));
                                    }
                                    if (arg.AllowedValues.Count > 0)
                                    {
                                        string argAllowed = string.Format("\t\t\t\tAllowed Values: \u0002{0}\u0002", string.Join(", ", arg.AllowedValues));
                                        bool containsAllowed = false;
                                        for (int x = 0; x < index; x++)
                                        {
                                            if (argDescriptions.ContainsKey(arg.Name + " - Allowed Values " + x))
                                            {
                                                if (argDescriptions[arg.Name + " - Allowed Values " + x] == argAllowed)
                                                {
                                                    containsAllowed = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (!containsAllowed)
                                        {
                                            argDescriptions.Add(arg.Name + " - Allowed Values " + index, argAllowed);
                                            index++;
                                        }
                                    }
                                });
                            }
                        }
                        Bot.IRC.Command.SendNotice(command.Nick.Nickname, "\u200B");
                        if (argDescriptions.Count > 0)
                        {
                            foreach (KeyValuePair<string, string> argDescription in argDescriptions)
                            {
                                Bot.IRC.Command.SendNotice(command.Nick.Nickname, argDescription.Value);
                            }
                        }
                    }
                    else
                    {
                        Bot.IRC.Command.SendNotice(command.Nick.Nickname, string.Format("You do not have access to view help on \u0002{0}\u0002.", helpCommand));
                    }
                }
                else
                {
                    Bot.IRC.Command.SendNotice(command.Nick.Nickname, string.Format("The command \u0002{0}\u0002 does not exist.", helpCommand));
                }
            }
            else
            {
                Bot.IRC.Command.SendNotice(command.Nick.Nickname, string.Format("The command \u0002{0}\u0002 does not exist.", helpCommand));
            }
        }
    }
}
