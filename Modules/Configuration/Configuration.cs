using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;

namespace Combot.Modules.Plugins
{
    public class Configuration : Module
    {
        private ReaderWriterLockSlim ConfigLock;

        public override void Initialize()
        {
            ConfigLock = new ReaderWriterLockSlim();
            Bot.CommandReceivedEvent += HandleCommandEvent;
        }

        public override void ParseCommand(CommandMessage command)
        {
            Command foundCommand = Commands.Find(c => c.Triggers.Contains(command.Command));
            switch (foundCommand.Name)
            {
                case "Command Configuration":
                    string parameter = command.Arguments["Parameter"];
                    Module mod = Bot.Modules.Find(module => module.Commands.Exists(c => c.Triggers.Contains(command.Arguments["Command"]) || c.Name == command.Arguments["Command"]));
                    if (mod != null)
                    {
                        Command cmd = mod.Commands.Find(c => c.Triggers.Contains(command.Arguments["Command"]) || c.Name == command.Arguments["Command"]);
                        string action = command.Arguments["Action"];
                        switch (parameter)
                        {
                            case "name":
                                switch (action.ToLower())
                                {
                                    case "edit":
                                        ConfigLock.EnterWriteLock();
                                        cmd.Name = command.Arguments["Value"];
                                        mod.SaveConfig();
                                        string editMessage = string.Format("Command Name is now: \u0002{0}\u0002", cmd.Name);
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, editMessage);
                                        ConfigLock.ExitWriteLock();
                                        break;
                                    case "view":
                                        string viewMessage = string.Format("Command Name: \u0002{0}\u0002", cmd.Name);
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, viewMessage);
                                        break;
                                }
                                break;
                            case "description":
                                switch (action.ToLower())
                                {
                                    case "edit":
                                        ConfigLock.EnterWriteLock();
                                        cmd.Description = command.Arguments["Value"];
                                        mod.SaveConfig();
                                        string editMessage = string.Format("\u0002{0}\u0002 Description is now: \u0002{1}\u0002", cmd.Name, cmd.Description);
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, editMessage);
                                        ConfigLock.ExitWriteLock();
                                        break;
                                    case "view":
                                        string viewMessage = string.Format("{0} Description: \u0002{1}\u0002", cmd.Name, cmd.Description);
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, viewMessage);
                                        break;
                                }
                                break;
                            case "enabled":
                                switch (action.ToLower())
                                {
                                    case "edit":
                                        ConfigLock.EnterWriteLock();
                                        cmd.Enabled = (bool) command.Arguments["Value"];
                                        mod.SaveConfig();
                                        string editMessage = string.Format("\u0002{0}\u0002 Enabled is now: \u0002{1}\u0002", cmd.Name, cmd.Enabled);
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, editMessage);
                                        ConfigLock.ExitWriteLock();
                                        break;
                                    case "view":
                                        string viewMessage = string.Format("{0} Enabled: \u0002{1}\u0002", cmd.Name, cmd.Enabled);
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, viewMessage);
                                        break;
                                }
                                break;
                            case "channelblacklist":
                                switch (action.ToLower())
                                {
                                    case "add":
                                        ConfigLock.EnterWriteLock();
                                        if (!cmd.ChannelBlacklist.Contains(command.Arguments["Value"]))
                                        {
                                            cmd.ChannelBlacklist.Add(command.Arguments["Value"]);
                                            mod.SaveConfig();
                                        }
                                        string addMessage = string.Format("\u0002{0}\u0002 Channel Blacklist is now: \u0002{1}\u0002", cmd.Name, string.Join(", ", cmd.ChannelBlacklist));
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, addMessage);
                                        ConfigLock.ExitWriteLock();
                                        break;
                                    case "del":
                                        ConfigLock.EnterWriteLock();
                                        if (cmd.ChannelBlacklist.Contains(command.Arguments["Value"]))
                                        {
                                            cmd.ChannelBlacklist.Remove(command.Arguments["Value"]);
                                            mod.SaveConfig();
                                        }
                                        string delMessage = string.Format("\u0002{0}\u0002 Channel Blacklist is now: \u0002{1}\u0002", cmd.Name, string.Join(", ", cmd.ChannelBlacklist));
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, delMessage);
                                        ConfigLock.ExitWriteLock();
                                        break;
                                    case "view":
                                        string viewMessage = string.Format("{0} Channel Blacklist: \u0002{1}\u0002", cmd.Name, string.Join(", ", cmd.ChannelBlacklist));
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, viewMessage);
                                        break;
                                }
                                break;
                            case "nickblacklist":
                                switch (action.ToLower())
                                {
                                    case "add":
                                        ConfigLock.EnterWriteLock();
                                        if (!cmd.NickBlacklist.Contains(command.Arguments["Value"]))
                                        {
                                            cmd.NickBlacklist.Add(command.Arguments["Value"]);
                                            mod.SaveConfig();
                                        }
                                        string addMessage = string.Format("\u0002{0}\u0002 Nickname Blacklist is now: \u0002{1}\u0002", cmd.Name, string.Join(", ", cmd.NickBlacklist));
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, addMessage);
                                        ConfigLock.ExitWriteLock();
                                        break;
                                    case "del":
                                        ConfigLock.EnterWriteLock();
                                        if (cmd.NickBlacklist.Contains(command.Arguments["Value"]))
                                        {
                                            cmd.NickBlacklist.Remove(command.Arguments["Value"]);
                                            mod.SaveConfig();
                                        }
                                        string delMessage = string.Format("\u0002{0}\u0002 Nickname Blacklist is now: \u0002{1}\u0002", cmd.Name, string.Join(", ", cmd.NickBlacklist));
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, delMessage);
                                        ConfigLock.ExitWriteLock();
                                        break;
                                    case "view":
                                        string viewMessage = string.Format("{0} Nickname Blacklist: \u0002{1}\u0002", cmd.Name, string.Join(", ", cmd.NickBlacklist));
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, viewMessage);
                                        break;
                                }
                                break;
                            case "triggers":
                                switch (action.ToLower())
                                {
                                    case "add":
                                        ConfigLock.EnterWriteLock();
                                        if (!cmd.Triggers.Contains(command.Arguments["Value"]))
                                        {
                                            cmd.Triggers.Add(command.Arguments["Value"]);
                                            mod.SaveConfig();
                                        }
                                        string addMessage = string.Format("\u0002{0}\u0002 Triggers are now: \u0002{1}\u0002", cmd.Name, string.Join(", ", cmd.Triggers));
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, addMessage);
                                        ConfigLock.ExitWriteLock();
                                        break;
                                    case "del":
                                        ConfigLock.EnterWriteLock();
                                        if (cmd.Triggers.Contains(command.Arguments["Value"]))
                                        {
                                            cmd.Triggers.Remove(command.Arguments["Value"]);
                                            mod.SaveConfig();
                                        }
                                        string delMessage = string.Format("\u0002{0}\u0002 Triggers are now: \u0002{1}\u0002", cmd.Name, string.Join(", ", cmd.Triggers));
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, delMessage);
                                        ConfigLock.ExitWriteLock();
                                        break;
                                    case "view":
                                        string viewMessage = string.Format("{0} Triggers: \u0002{1}\u0002", cmd.Name, string.Join(", ", cmd.Triggers));
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, viewMessage);
                                        break;
                                }
                                break;
                            case "arguments":
                                switch (action.ToLower())
                                {
                                    case "add":
                                        CommandArgument addArg = JsonConvert.DeserializeObject<CommandArgument>(command.Arguments["Value"]);
                                        if (addArg != null)
                                        {
                                            ConfigLock.EnterWriteLock();
                                            if (!cmd.Arguments.Contains(addArg))
                                            {
                                                cmd.Arguments.Add(addArg);
                                                mod.SaveConfig();
                                            }
                                            string addMessage = string.Format("\u0002{0}\u0002 Arguments are now:", cmd.Name);
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, addMessage);
                                            foreach (CommandArgument argument in cmd.Arguments)
                                            {
                                                addMessage = string.Format("Argument: \u0002{0}\u0002", JsonConvert.SerializeObject(argument));
                                                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, addMessage);
                                            }
                                            ConfigLock.ExitWriteLock();
                                        }
                                        else
                                        {
                                            string notValid = string.Format("\u0002{0}\u000F is not a valid argument.", command.Arguments["Value"]);
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, notValid);
                                        }
                                        break;
                                    case "del":
                                        CommandArgument delArg = JsonConvert.DeserializeObject<CommandArgument>(command.Arguments["Value"]);
                                        if (delArg != null)
                                        {
                                            ConfigLock.EnterWriteLock();
                                            if (cmd.Arguments.Contains(delArg))
                                            {
                                                cmd.Arguments.Remove(delArg);
                                                mod.SaveConfig();
                                            }
                                            string delMessage = string.Format("\u0002{0}\u0002 Arguments are now:", cmd.Name);
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, delMessage);
                                            foreach (CommandArgument argument in cmd.Arguments)
                                            {
                                                delMessage = string.Format("Argument: \u0002{0}\u0002", JsonConvert.SerializeObject(argument));
                                                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, delMessage);
                                            }
                                            ConfigLock.ExitWriteLock();
                                        }
                                        else
                                        {
                                            string notValid = string.Format("\u0002{0}\u000F is not a valid argument.", command.Arguments["Value"]);
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, notValid);
                                        }
                                        break;
                                    case "view":
                                        foreach (CommandArgument argument in cmd.Arguments)
                                        {
                                            string viewMessage = string.Format("{0} Argument: \u0002{1}\u0002", cmd.Name, JsonConvert.SerializeObject(argument));
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, viewMessage);
                                        }
                                        break;
                                }
                                break;
                            case "allowedmessagetypes":
                                switch (action.ToLower())
                                {
                                    case "add":
                                        MessageType addType;
                                        bool addValid = Enum.TryParse(command.Arguments["Value"], out addType);
                                        if (addValid)
                                        {
                                            ConfigLock.EnterWriteLock();
                                            if (!cmd.AllowedMessageTypes.Contains(addType))
                                            {
                                                cmd.AllowedMessageTypes.Add(addType);
                                                mod.SaveConfig();
                                            }
                                            mod.SaveConfig();
                                            List<string> addAllowedTypes = new List<string>();
                                            cmd.AllowedMessageTypes.ForEach(type => addAllowedTypes.Add(type.ToString()));
                                            string addMessage = string.Format("\u0002{0}\u0002 Allowed Message Types are now: \u0002{1}\u0002", cmd.Name, string.Join(", ", addAllowedTypes));
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, addMessage);
                                            ConfigLock.ExitWriteLock();
                                        }
                                        else
                                        {
                                            string notValid = string.Format("\u0002{0}\u000F is not a valid message type.", command.Arguments["Value"]);
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, notValid);
                                        }
                                        break;
                                    case "del":
                                        MessageType delType;
                                        bool delValid = Enum.TryParse(command.Arguments["Value"], out delType);
                                        if (delValid)
                                        {
                                            ConfigLock.EnterWriteLock();
                                            if (cmd.AllowedMessageTypes.Contains(delType))
                                            {
                                                cmd.AllowedMessageTypes.Remove(delType);
                                                mod.SaveConfig();
                                            }
                                            List<string> delAllowedTypes = new List<string>();
                                            cmd.AllowedMessageTypes.ForEach(type => delAllowedTypes.Add(type.ToString()));
                                            string delMessage = string.Format("\u0002{0}\u0002 Allowed Message Types are now: \u0002{1}\u0002", cmd.Name, string.Join(", ", delAllowedTypes));
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, delMessage);
                                            ConfigLock.ExitWriteLock();
                                        }
                                        else
                                        {
                                            string notValid = string.Format("\u0002{0}\u000F is not a valid message type.", command.Arguments["Value"]);
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, notValid);
                                        }
                                        break;
                                    case "view":
                                        List<string> allowedTypes = new List<string>();
                                        cmd.AllowedMessageTypes.ForEach(type => allowedTypes.Add(type.ToString()));
                                        string viewMessage = string.Format("{0} Allowed Message Types: \u0002{1}\u0002", cmd.Name, string.Join(", ", allowedTypes));
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, viewMessage);
                                        break;
                                }
                                break;
                            case "allowedaccess":
                                switch (action.ToLower())
                                {
                                    case "add":
                                        AccessType addType;
                                        bool addValid = Enum.TryParse(command.Arguments["Value"], out addType);
                                        if (addValid)
                                        {
                                            ConfigLock.EnterWriteLock();
                                            if (!cmd.AllowedAccess.Contains(addType))
                                            {
                                                cmd.AllowedAccess.Add(addType);
                                                mod.SaveConfig();
                                            }
                                            List<string> addAllowedAccess = new List<string>();
                                            cmd.AllowedAccess.ForEach(access => addAllowedAccess.Add(access.ToString()));
                                            string addMessage = string.Format("\u0002{0}\u0002 Allowed Accesses are now: \u0002{1}\u0002", cmd.Name, string.Join(", ", addAllowedAccess));
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, addMessage);
                                            ConfigLock.ExitWriteLock();
                                        }
                                        else
                                        {
                                            string notValid = string.Format("\u0002{0}\u000F is not a valid access level.", command.Arguments["Value"]);
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, notValid);
                                        }
                                        break;
                                    case "del":
                                        AccessType delType;
                                        bool delValid = Enum.TryParse(command.Arguments["Value"], out delType);
                                        if (delValid)
                                        {
                                            ConfigLock.EnterWriteLock();
                                            if (cmd.AllowedAccess.Contains(delType))
                                            {
                                                cmd.AllowedAccess.Remove(delType);
                                                mod.SaveConfig();
                                            }
                                            List<string> delAllowedAccess = new List<string>();
                                            cmd.AllowedAccess.ForEach(access => delAllowedAccess.Add(access.ToString()));
                                            string delMessage = string.Format("\u0002{0}\u0002 Allowed Accesses are now: \u0002{1}\u0002", cmd.Name, string.Join(", ", delAllowedAccess));
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, delMessage);
                                            ConfigLock.ExitWriteLock();
                                        }
                                        else
                                        {
                                            string notValid = string.Format("\u0002{0}\u000F is not a valid access level.", command.Arguments["Value"]);
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, notValid);
                                        }
                                        break;
                                    case "view":
                                        List<string> allowedAccess = new List<string>();
                                        cmd.AllowedAccess.ForEach(type => allowedAccess.Add(type.ToString()));
                                        string viewMessage = string.Format("{0} Allowed Accesses: \u0002{1}\u0002", cmd.Name, string.Join(", ", allowedAccess));
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, viewMessage);
                                        break;
                                }
                                break;
                            case "showhelp":
                                switch (action.ToLower())
                                {
                                    case "edit":
                                        ConfigLock.EnterWriteLock();
                                        cmd.ShowHelp = (bool)command.Arguments["Value"];
                                        mod.SaveConfig();
                                        string editMessage = string.Format("\u0002{0}\u0002 Show Help is now: \u0002{1}\u0002", cmd.Name, cmd.ShowHelp);
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, editMessage);
                                        ConfigLock.ExitWriteLock();
                                        break;
                                    case "view":
                                        string viewMessage = string.Format("{0} Show Help: \u0002{1}\u0002", cmd.Name, cmd.ShowHelp);
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, viewMessage);
                                        break;
                                }
                                break;
                            case "spamcheck":
                                switch (action.ToLower())
                                {
                                    case "edit":
                                        ConfigLock.EnterWriteLock();
                                        cmd.SpamCheck = (bool)command.Arguments["Value"];
                                        mod.SaveConfig();
                                        string editMessage = string.Format("\u0002{0}\u0002 Spam Check is now: \u0002{1}\u0002", cmd.Name, cmd.SpamCheck);
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, editMessage);
                                        ConfigLock.ExitWriteLock();
                                        break;
                                    case "view":
                                        string viewMessage = string.Format("{0} Spam Check: \u0002{1}\u0002", cmd.Name, cmd.SpamCheck);
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, viewMessage);
                                        break;
                                }
                                break;
                        }
                    }
                    else
                    {
                        string notFound = string.Format("\u0002{0}\u000F is not a valid command.", command.Arguments["Command"]);
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, notFound);
                    }
                    break;
                case "Module Configuration":
                    string moduleParameter = command.Arguments["Parameter"];
                    Module foundModule = Bot.Modules.Find(module => module.Name == command.Arguments["Module"] || module.ClassName == command.Arguments["Module"]);
                    if (foundModule != null)
                    {
                        string action = command.Arguments["Action"];
                        switch (moduleParameter)
                        {
                            case "name":
                                switch (action.ToLower())
                                {
                                    case "edit":
                                        ConfigLock.EnterWriteLock();
                                        foundModule.Name = command.Arguments["Value"];
                                        foundModule.SaveConfig();
                                        string editMessage = string.Format("Module Name is now: \u0002{0}\u0002", foundModule.Name);
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, editMessage);
                                        ConfigLock.ExitWriteLock();
                                        break;
                                    case "view":
                                        string viewMessage = string.Format("Module Name: \u0002{0}\u0002", foundModule.Name);
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, viewMessage);
                                        break;
                                }
                                break;
                            case "classname":
                                switch (action.ToLower())
                                {
                                    case "edit":
                                        ConfigLock.EnterWriteLock();
                                        foundModule.ClassName = command.Arguments["Value"];
                                        foundModule.SaveConfig();
                                        string editMessage = string.Format("\u0002{0}\u0002 Class Name is now: \u0002{1}\u0002", foundModule.Name, foundModule.ClassName);
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, editMessage);
                                        ConfigLock.ExitWriteLock();
                                        break;
                                    case "view":
                                        string viewMessage = string.Format("{0} Description: \u0002{1}\u0002", foundModule.Name, foundModule.ClassName);
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, viewMessage);
                                        break;
                                }
                                break;
                            case "enabled":
                                switch (action.ToLower())
                                {
                                    case "edit":
                                        ConfigLock.EnterWriteLock();
                                        foundModule.Enabled = (bool)command.Arguments["Value"];
                                        foundModule.SaveConfig();
                                        string editMessage = string.Format("\u0002{0}\u0002 Enabled is now: \u0002{1}\u0002", foundModule.Name, foundModule.Enabled);
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, editMessage);
                                        ConfigLock.ExitWriteLock();
                                        break;
                                    case "view":
                                        string viewMessage = string.Format("{0} Enabled: \u0002{1}\u0002", foundModule.Name, foundModule.Enabled);
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, viewMessage);
                                        break;
                                }
                                break;
                            case "channelblacklist":
                                switch (action.ToLower())
                                {
                                    case "add":
                                        ConfigLock.EnterWriteLock();
                                        if (!foundModule.ChannelBlacklist.Contains(command.Arguments["Value"]))
                                        {
                                            foundModule.ChannelBlacklist.Add(command.Arguments["Value"]);
                                            foundModule.SaveConfig();
                                        }
                                        string addMessage = string.Format("\u0002{0}\u0002 Channel Blacklist is now: \u0002{1}\u0002", foundModule.Name, string.Join(", ", foundModule.ChannelBlacklist));
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, addMessage);
                                        ConfigLock.ExitWriteLock();
                                        break;
                                    case "del":
                                        ConfigLock.EnterWriteLock();
                                        if (foundModule.ChannelBlacklist.Contains(command.Arguments["Value"]))
                                        {
                                            foundModule.ChannelBlacklist.Remove(command.Arguments["Value"]);
                                            foundModule.SaveConfig();
                                        }
                                        string delMessage = string.Format("\u0002{0}\u0002 Channel Blacklist is now: \u0002{1}\u0002", foundModule.Name, string.Join(", ", foundModule.ChannelBlacklist));
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, delMessage);
                                        ConfigLock.ExitWriteLock();
                                        break;
                                    case "view":
                                        string viewMessage = string.Format("{0} Channel Blacklist: \u0002{1}\u0002", foundModule.Name, string.Join(", ", foundModule.ChannelBlacklist));
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, viewMessage);
                                        break;
                                }
                                break;
                            case "nickblacklist":
                                switch (action.ToLower())
                                {
                                    case "add":
                                        ConfigLock.EnterWriteLock();
                                        if (!foundModule.NickBlacklist.Contains(command.Arguments["Value"]))
                                        {
                                            foundModule.NickBlacklist.Add(command.Arguments["Value"]);
                                            foundModule.SaveConfig();
                                        }
                                        string addMessage = string.Format("\u0002{0}\u0002 Nickname Blacklist is now: \u0002{1}\u0002", foundModule.Name, string.Join(", ", foundModule.NickBlacklist));
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, addMessage);
                                        ConfigLock.ExitWriteLock();
                                        break;
                                    case "del":
                                        ConfigLock.EnterWriteLock();
                                        if (foundModule.NickBlacklist.Contains(command.Arguments["Value"]))
                                        {
                                            foundModule.NickBlacklist.Remove(command.Arguments["Value"]);
                                            foundModule.SaveConfig();
                                        }
                                        string delMessage = string.Format("\u0002{0}\u0002 Nickname Blacklist is now: \u0002{1}\u0002", foundModule.Name, string.Join(", ", foundModule.NickBlacklist));
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, delMessage);
                                        ConfigLock.ExitWriteLock();
                                        break;
                                    case "view":
                                        string viewMessage = string.Format("{0} Nickname Blacklist: \u0002{1}\u0002", foundModule.Name, string.Join(", ", foundModule.NickBlacklist));
                                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, viewMessage);
                                        break;
                                }
                                break;
                            case "commands":
                                switch (action.ToLower())
                                {
                                    case "add":
                                        Command addCommand = JsonConvert.DeserializeObject<Command>(command.Arguments["Value"]);
                                        if (addCommand != null)
                                        {
                                            ConfigLock.EnterWriteLock();
                                            if (!foundModule.Commands.Contains(addCommand))
                                            {
                                                foundModule.Commands.Add(addCommand);
                                                foundModule.SaveConfig();
                                            }
                                            string addMessage = string.Format("\u0002{0}\u0002 Commands are now:", foundModule.Name);
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, addMessage);
                                            foreach (Command cmd in foundModule.Commands)
                                            {
                                                addMessage = string.Format("Command: \u0002{0}\u0002", JsonConvert.SerializeObject(cmd));
                                                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, addMessage);
                                            }
                                            ConfigLock.ExitWriteLock();
                                        }
                                        else
                                        {
                                            string notValid = string.Format("\u0002{0}\u000F is not a valid command.", command.Arguments["Value"]);
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, notValid);
                                        }
                                        break;
                                    case "del":
                                        Command delCommand = JsonConvert.DeserializeObject<Command>(command.Arguments["Value"]);
                                        if (delCommand != null)
                                        {
                                            ConfigLock.EnterWriteLock();
                                            if (foundModule.Commands.Contains(delCommand))
                                            {
                                                foundModule.Commands.Remove(delCommand);
                                                foundModule.SaveConfig();
                                            }
                                            string delMessage = string.Format("\u0002{0}\u0002 Commands are now:", foundModule.Name);
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, delMessage);
                                            foreach (Command cmd in foundModule.Commands)
                                            {
                                                delMessage = string.Format("Command: \u0002{0}\u0002", JsonConvert.SerializeObject(cmd));
                                                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, delMessage);
                                            }
                                            ConfigLock.ExitWriteLock();
                                        }
                                        else
                                        {
                                            string notValid = string.Format("\u0002{0}\u000F is not a valid command.", command.Arguments["Value"]);
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, notValid);
                                        }
                                        break;
                                    case "view":
                                        foreach (Command cmd in foundModule.Commands)
                                        {
                                            string viewMessage = string.Format("{0} Command: \u0002{1}\u0002", foundModule.Name, JsonConvert.SerializeObject(cmd));
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, viewMessage);
                                        }
                                        break;
                                }
                                break;
                            case "options":
                                switch (action.ToLower())
                                {
                                    case "add":
                                        Option addOption = JsonConvert.DeserializeObject<Option>(command.Arguments["Value"]);
                                        if (addOption != null)
                                        {
                                            ConfigLock.EnterWriteLock();
                                            if (!foundModule.Options.Contains(addOption))
                                            {
                                                foundModule.Options.Add(addOption);
                                                foundModule.SaveConfig();
                                            }
                                            string addMessage = string.Format("\u0002{0}\u0002 Options are now:", foundModule.Name);
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, addMessage);
                                            foreach (Option opt in foundModule.Options)
                                            {
                                                addMessage = string.Format("Option: \u0002{0}\u0002", JsonConvert.SerializeObject(opt));
                                                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, addMessage);
                                            }
                                            ConfigLock.ExitWriteLock();
                                        }
                                        else
                                        {
                                            string notValid = string.Format("\u0002{0}\u000F is not a valid option.", command.Arguments["Value"]);
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, notValid);
                                        }
                                        break;
                                    case "del":
                                        Option delOption = JsonConvert.DeserializeObject<Option>(command.Arguments["Value"]);
                                        if (delOption != null)
                                        {
                                            ConfigLock.EnterWriteLock();
                                            if (foundModule.Options.Contains(delOption))
                                            {
                                                foundModule.Options.Remove(delOption);
                                                foundModule.SaveConfig();
                                            }
                                            string delMessage = string.Format("\u0002{0}\u0002 Options are now:", foundModule.Name);
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, delMessage);
                                            foreach (Option opt in foundModule.Options)
                                            {
                                                delMessage = string.Format("Option: \u0002{0}\u0002", JsonConvert.SerializeObject(opt));
                                                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, delMessage);
                                            }
                                            ConfigLock.ExitWriteLock();
                                        }
                                        else
                                        {
                                            string notValid = string.Format("\u0002{0}\u000F is not a valid option.", command.Arguments["Value"]);
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, notValid);
                                        }
                                        break;
                                    case "view":
                                        foreach (Option opt in foundModule.Options)
                                        {
                                            string viewMessage = string.Format("{0} Command: \u0002{1}\u0002", foundModule.Name, JsonConvert.SerializeObject(opt));
                                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, viewMessage);
                                        }
                                        break;
                                }
                                break;
                        }
                    }
                    else
                    {
                        string notFound = string.Format("\u0002{0}\u000F is not a valid module.", command.Arguments["Module"]);
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, notFound);
                    }
                    break;
            }
        }
    }
}
