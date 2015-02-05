using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Combot.Modules.ModuleClasses
{
    public class Owner : Module
    {
        public override void Initialize()
        {
            Bot.CommandReceivedEvent += HandleCommandEvent;
        }

        public override void ParseCommand(CommandMessage command)
        {
            Command foundCommand = Commands.Find(c => c.Triggers.Contains(command.Command));
            switch (foundCommand.Name)
            {
                case "Change Nick":
                    Bot.IRC.SendNick(command.Arguments["Nickname"]);
                    break;
                case "Identify":
                    Bot.IRC.SendPrivateMessage("NickServ", string.Format("Identify {0}", Bot.ServerConfig.Password));
                    break;
                case "Join Channel":
                    if (!Bot.IRC.Channels.Exists(chan => chan.Name == command.Arguments["Channel"]))
                    {
                        Bot.IRC.SendJoin(command.Arguments["Channel"]);
                    }
                    else
                    {
                        string message = string.Format("I am already in \u0002{0}\u000F.", command.Arguments["Channel"]);
                        switch (command.MessageType)
                        {
                            case MessageType.Channel:
                                Bot.IRC.SendPrivateMessage(command.Location, message);
                                break;
                            case MessageType.Query:
                                Bot.IRC.SendPrivateMessage(command.Nick.Nickname, message);
                                break;
                            case MessageType.Notice:
                                Bot.IRC.SendNotice(command.Nick.Nickname, message);
                                break;
                        }
                    }
                    break;
                case "Part Channel":
                    string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
                    if (Bot.IRC.Channels.Exists(chan => chan.Name == channel))
                    {
                        Bot.IRC.SendPart(channel);
                    }
                    else
                    {
                        string message = string.Format("I am not in \u0002{0}\u000F.", channel);
                        switch (command.MessageType)
                        {
                            case MessageType.Channel:
                                Bot.IRC.SendPrivateMessage(command.Location, message);
                                break;
                            case MessageType.Query:
                                Bot.IRC.SendPrivateMessage(command.Nick.Nickname, message);
                                break;
                            case MessageType.Notice:
                                Bot.IRC.SendNotice(command.Nick.Nickname, message);
                                break;
                        }
                    }
                    break;
                case "Speak":
                    string location = command.Arguments.ContainsKey("Target") ? command.Arguments["Target"] : command.Location;
                    Bot.IRC.SendPrivateMessage(location, command.Arguments["Message"]);
                    break;
                case "Action":
                    string actionLocation = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
                    Bot.IRC.SendCTCPMessage(actionLocation, "ACTION", command.Arguments["Message"]);
                    break;
                case "Quit":
                    string quitType = command.Arguments["Type"].ToString();
                    switch (quitType.ToLower())
                    {
                        case "server":
                            Bot.Disconnect();
                            break;
                        case "client":
                            Environment.Exit(0);
                            break;
                    }
                    break;
                case "Cycle":
                    string cycleType = command.Arguments["Type"].ToString();
                    switch (cycleType.ToLower())
                    {
                        case "channel":
                            string cycleChannel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
                            if (Bot.IRC.Channels.Exists(chan => chan.Name == cycleChannel))
                            {
                                Bot.IRC.SendPart(cycleChannel);
                                Bot.IRC.SendJoin(cycleChannel);
                            }
                            else
                            {
                                string message = string.Format("I am not in \u0002{0}\u000F.", cycleChannel);
                                switch (command.MessageType)
                                {
                                    case MessageType.Channel:
                                        Bot.IRC.SendPrivateMessage(command.Location, message);
                                        break;
                                    case MessageType.Query:
                                        Bot.IRC.SendPrivateMessage(command.Nick.Nickname, message);
                                        break;
                                    case MessageType.Notice:
                                        Bot.IRC.SendNotice(command.Nick.Nickname, message);
                                        break;
                                }
                            }
                            break;
                        case "server":
                            Bot.Disconnect();
                            Bot.Connect();
                            break;
                        case "client":
                            Process.Start(Assembly.GetEntryAssembly().Location); // to start new instance of application
                            Environment.Exit(0);
                            break;
                    }
                    break;
                case "List":
                    string listType = command.Arguments["Type"].ToString();
                    string listLocation = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
                    switch (listType.ToLower())
                    {
                        case "nicks":
                            if (Bot.IRC.Channels.Exists(chan => chan.Name == listLocation))
                            {
                                string nickList = string.Join(", ", Bot.IRC.Channels.Find(chan => chan.Name == listLocation).Nicks.Select(nick => nick.Nickname));
                                string nickMessage = string.Format("Nicknames in \u0002{0}\u000F: {1}", listLocation, nickList);
                                switch (command.MessageType)
                                {
                                    case MessageType.Channel:
                                        Bot.IRC.SendPrivateMessage(command.Location, nickMessage);
                                        break;
                                    case MessageType.Query:
                                        Bot.IRC.SendPrivateMessage(command.Nick.Nickname, nickMessage);
                                        break;
                                    case MessageType.Notice:
                                        Bot.IRC.SendNotice(command.Nick.Nickname, nickMessage);
                                        break;
                                }
                            }
                            else
                            {
                                string message = string.Format("I do not have nickname information for \u0002{0}\u000F.", listLocation);
                                switch (command.MessageType)
                                {
                                    case MessageType.Channel:
                                        Bot.IRC.SendPrivateMessage(command.Location, message);
                                        break;
                                    case MessageType.Query:
                                        Bot.IRC.SendPrivateMessage(command.Nick.Nickname, message);
                                        break;
                                    case MessageType.Notice:
                                        Bot.IRC.SendNotice(command.Nick.Nickname, message);
                                        break;
                                }
                            }
                            break;
                        case "channels":
                            string channelList = string.Join(", ", Bot.IRC.Channels.Select(chan => chan.Name));
                            string channelMessage = string.Format("I am in the following channels: \u0002{0}\u000F", channelList);
                            switch (command.MessageType)
                            {
                                case MessageType.Channel:
                                    Bot.IRC.SendPrivateMessage(command.Location, channelMessage);
                                    break;
                                case MessageType.Query:
                                    Bot.IRC.SendPrivateMessage(command.Nick.Nickname, channelMessage);
                                    break;
                                case MessageType.Notice:
                                    Bot.IRC.SendNotice(command.Nick.Nickname, channelMessage);
                                    break;
                            }
                            break;
                        case "servers":
                            // TODO Add server list
                            break;
                        case "modules":
                            string moduleList = string.Join(", ", Bot.Modules.Select(module => module.Name));
                            string moduleMessage = string.Format("I have the following modules loaded: \u0002{0}\u000F", moduleList);
                            switch (command.MessageType)
                            {
                                case MessageType.Channel:
                                    Bot.IRC.SendPrivateMessage(command.Location, moduleMessage);
                                    break;
                                case MessageType.Query:
                                    Bot.IRC.SendPrivateMessage(command.Nick.Nickname, moduleMessage);
                                    break;
                                case MessageType.Notice:
                                    Bot.IRC.SendNotice(command.Nick.Nickname, moduleMessage);
                                    break;
                            }
                            break;
                    }
                    break;
            }
        }
    }
}