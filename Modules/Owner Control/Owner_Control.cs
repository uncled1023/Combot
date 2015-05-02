using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Combot.IRCServices;

namespace Combot.Modules.Plugins
{
    public class Owner_Control : Module
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
                case "Owner Identify":
                    if (command.Arguments["Password"] == Bot.ServerConfig.Password)
                    {
                        if (!Bot.ServerConfig.Owners.Contains(command.Nick.Nickname))
                        {
                            Bot.ServerConfig.Owners.Add(command.Nick.Nickname);
                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, "You are now identified as an owner.");
                        }
                        else
                        {
                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, "You are already identified as an owner.");
                        }
                        foreach (Channel chan in Bot.IRC.Channels)
                        {
                            Nick foundNick = chan.Nicks.Find(nick => nick.Nickname == command.Nick.Nickname);
                            if (foundNick != null)
                            {
                                foundNick.AddMode(UserMode.r);
                            }
                        }
                    }
                    break;
                case "Change Nick":
                    Bot.IRC.Command.SendNick(command.Arguments["Nickname"]);
                    break;
                case "Identify":
                    Bot.IRC.Command.SendPrivateMessage("NickServ", string.Format("Identify {0}", Bot.ServerConfig.Password));
                    break;
                case "Join Channel":
                    if (!Bot.IRC.Channels.Exists(chan => chan.Name == command.Arguments["Channel"]))
                    {
                        Bot.IRC.Command.SendJoin(command.Arguments["Channel"]);
                    }
                    else
                    {
                        string message = string.Format("I am already in \u0002{0}\u000F.", command.Arguments["Channel"]);
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, message);
                    }
                    break;
                case "Part Channel":
                    string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
                    if (Bot.IRC.Channels.Exists(chan => chan.Name == channel))
                    {
                        Bot.IRC.Command.SendPart(channel);
                    }
                    else
                    {
                        string message = string.Format("I am not in \u0002{0}\u000F.", channel);
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, message);
                    }
                    break;
                case "Speak":
                    string location = command.Arguments.ContainsKey("Target") ? command.Arguments["Target"] : command.Location;
                    Bot.IRC.Command.SendPrivateMessage(location, command.Arguments["Message"]);
                    break;
                case "Action":
                    string actionLocation = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
                    Bot.IRC.Command.SendCTCPMessage(actionLocation, "ACTION", command.Arguments["Message"]);
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
                                Bot.IRC.Command.SendPart(cycleChannel);
                                Bot.IRC.Command.SendJoin(cycleChannel);
                            }
                            else
                            {
                                string message = string.Format("I am not in \u0002{0}\u000F.", cycleChannel);
                                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, message);
                            }
                            break;
                        case "server":
                            Bot.Disconnect();
                            Thread.Sleep(1000);
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
                                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, nickMessage);
                            }
                            else
                            {
                                string message = string.Format("I do not have nickname information for \u0002{0}\u000F.", listLocation);
                                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, message);
                            }
                            break;
                        case "channels":
                            string channelList = string.Join(", ", Bot.IRC.Channels.Select(chan => chan.Name));
                            string channelMessage = string.Format("I am in the following channels: \u0002{0}\u000F", channelList);
                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, channelMessage);
                            break;
                        case "servers":
                            string serverList = string.Empty;
                            foreach (Bot bot in Controller.Instance.Bots)
                            {
                                if (bot.Connected)
                                {
                                    serverList = string.Join(", ", serverList, bot.ServerConfig.Name);
                                }
                            }
                            if (string.IsNullOrEmpty(serverList))
                            {
                                serverList = "None Connected";
                            }
                            string serverMessage = string.Format("I am connected to the following servers: \u0002{0}\u000F", serverList.TrimStart(',').Trim());
                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, serverMessage);
                            break;
                        case "modules":
                            string moduleList = string.Join(", ", Bot.Modules.Select(module => module.Name));
                            string moduleMessage = string.Format("I have the following modules loaded: \u0002{0}\u000F", moduleList);
                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, moduleMessage);
                            break;
                    }
                    break;
                case "Load Module":
                    string loadModuleName = command.Arguments["Module"].ToString();
                    if (!Bot.Modules.Exists(mod => mod.Name.ToLower() == loadModuleName.ToLower()))
                    {
                        string modulePath = Path.Combine(Bot.ServerConfig.ModuleLocation, loadModuleName);
                        bool loaded = Bot.LoadModule(modulePath);
                        if (loaded)
                        {
                            string moduleMessage = string.Format("\u0002{0}\u000F has been loaded.", loadModuleName);
                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, moduleMessage);
                        }
                        else
                        {
                            string moduleMessage = string.Format("\u0002{0}\u000F was unable to be loaded.", loadModuleName);
                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, moduleMessage);
                        }
                    }
                    else
                    {
                        string moduleInvalid = string.Format("\u0002{0}\u000F is already loaded.", loadModuleName);
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, moduleInvalid);
                    }
                    break;
                case "Unload Module":
                    string unloadModuleName = command.Arguments["Module"].ToString();
                    if (Bot.Modules.Exists(mod => mod.Name.ToLower() == unloadModuleName.ToLower()))
                    {
                        bool unloaded = Bot.UnloadModule(unloadModuleName);
                        if (unloaded)
                        {
                            string moduleMessage = string.Format("\u0002{0}\u000F has been unloaded.", unloadModuleName);
                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, moduleMessage);
                        }
                        else
                        {
                            string moduleMessage = string.Format("\u0002{0}\u000F was unable to be unloaded.", unloadModuleName);
                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, moduleMessage);
                        }
                    }
                    else
                    {
                        string moduleInvalid = string.Format("\u0002{0}\u000F is not loaded.", unloadModuleName);
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, moduleInvalid);
                    }
                    break;
                case "Update":
                    Bot.ServerConfig.Load();
                    for (int i = 0; i < Bot.Modules.Count; i++)
                    {
                        Bot.Modules[i].LoadConfig();
                    }
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, "Update Complete");
                    break;
            }
        }
    }
}