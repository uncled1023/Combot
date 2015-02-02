using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;

namespace Combot.Modules.ModuleClasses
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
                    SendCommandHelp(command.Nick.Nickname, command.Access, command.Arguments["Command"].ToString());
                }
            }
        }

        private void SendFullHelp(string recipient, List<AccessType> access)
        {
            Bot.IRC.SendNotice(recipient, string.Format("You have the following commands available to use.  " +
                                                        "To use them either type \u0002{1}\u001Fcommand\u000F into a channel, send a private message by typing \u0002/msg {0} \u001Fcommand\u000F, or send a notice by typing \u0002/notice {0} \u001Fcommand\u000F.  " +
                                                        "For more information on a specific command, type \u0002{1}help \u001Fcommand\u000F.",
                                                        Bot.IRC.Nickname, Bot.ServerConfig.CommandPrefix));
            Bot.IRC.SendNotice(recipient, "\u200B");
            foreach (Module module in Bot.Modules)
            {
                if (module.Commands.Exists(command => command.AllowedAccess.Exists(allowed => access.Contains(allowed)) && command.ShowHelp))
                {
                    Bot.IRC.SendNotice(recipient, string.Format("\u0002\u001F{0} Module\u000F\u0002\u000F", module.Name));
                }
                module.Commands.ForEach(command =>
                {
                    if (command.AllowedAccess.Exists(allowed => access.Contains(allowed)) && command.ShowHelp)
                    {
                        string commandDesc = string.Empty;
                        if (command.Description != string.Empty)
                        {
                            commandDesc = string.Format(" - {0}", command.Description);
                        }
                        Bot.IRC.SendNotice(recipient, string.Format("\t\t\u0002{0}\u000F{1}", command.Name, commandDesc));
                    }
                });
            }
        }

        private void SendCommandHelp(string recipient, List<AccessType> access, string command)
        {
            Module foundModule = Bot.Modules.Find(mod => mod.Commands.Exists(cmd => (cmd.Name == command || cmd.Triggers.Contains(command)) && cmd.ShowHelp));
            if (foundModule != null)
            {
                Command foundCommand = foundModule.Commands.Find(cmd => (cmd.Name == command || cmd.Triggers.Contains(command)));
                if (foundCommand != null)
                {
                    if (foundCommand.AllowedAccess.Exists(allowed => access.Contains(allowed)))
                    {
                        Bot.IRC.SendNotice(recipient, string.Format("Help information for \u0002{0}\u000F", foundCommand.Name));
                        if (foundCommand.Description != string.Empty)
                        {
                            Bot.IRC.SendNotice(recipient, string.Format("{0}", foundCommand.Description));
                        }
                        Bot.IRC.SendNotice(recipient, "\u200B");
                        for (int i = 0; i < foundCommand.AllowedMessageTypes.Count; i++)
                        {
                            MessageType messageType = foundCommand.AllowedMessageTypes[i];
                            string messageSyntax = string.Empty;
                            switch (messageType)
                            {
                                case MessageType.Channel:
                                    messageSyntax = "\u0002/msg \u001Fchannel\u000F";
                                    break;
                                case MessageType.Query:
                                    messageSyntax = string.Format("\u0002/msg {0}\u000F", Bot.IRC.Nickname);
                                    break;
                                case MessageType.Notice:
                                    messageSyntax = string.Format("\u0002/notice {0}\u000F", Bot.IRC.Nickname);
                                    break;
                            }
                            List<CommandArgument> validArguments = foundCommand.Arguments.FindAll(arg => arg.MessageTypes.Contains(messageType));
                            string argHelp = string.Empty;
                            Bot.IRC.SendNotice(recipient, string.Format("Message Type: \u0002{0}\u000F", messageType.ToString()));
                            if (validArguments.Count > 0)
                            {
                                argHelp = string.Format(" \u0002\u001F{0}\u000F", string.Join("\u000F \u0002", validArguments.Select(arg =>
                                {
                                    if (arg.Required)
                                    {
                                        return "\u001F" + arg.Name + "\u000F\u0002";
                                    }
                                    return "[\u001F" + arg.Name + "\u000F\u0002]";
                                })));
                            }
                            foundCommand.Triggers.ForEach(trigger =>
                            {
                                Bot.IRC.SendNotice(recipient, string.Format("\t\tSyntax: {0} {1}\u0002{2}\u000F{3}", messageSyntax, Bot.ServerConfig.CommandPrefix, trigger, argHelp));
                            });

                            if (validArguments.Count > 0)
                            {
                                Bot.IRC.SendNotice(recipient, "\u200B");
                                validArguments.ForEach(arg =>
                                {
                                    string commandDesc = string.Empty;
                                    if (arg.Description != string.Empty)
                                    {
                                        commandDesc = string.Format(" - {0}", arg.Description);
                                    }
                                    string required = string.Empty;
                                    if (arg.Required)
                                    {
                                        required = " - Required";
                                    }
                                    Bot.IRC.SendNotice(recipient, string.Format("\t\t\u0002{0}{1}\u000F{2}", arg.Name, required, commandDesc));
                                    if (arg.AllowedValues.Count > 0)
                                    {
                                        Bot.IRC.SendNotice(recipient, string.Format("\t\t\t\t- Allowed Values: \u0002{0}\u000F", string.Join(", ", arg.AllowedValues)));
                                    }
                                });
                            }
                            Bot.IRC.SendNotice(recipient, "\u200B");
                        }
                    }
                    else
                    {
                        Bot.IRC.SendNotice(recipient, string.Format("You do not have access to view help on \u0002{0}\u000F.", command));
                    }
                }
                else
                {
                    Bot.IRC.SendNotice(recipient, string.Format("The command \u0002{0}\u000F does not exist.", command));
                }
            }
            else
            {
                Bot.IRC.SendNotice(recipient, string.Format("The command \u0002{0}\u000F does not exist.", command));
            }
        }
    }
}