using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Combot.IRCServices;
using Combot.Configurations;
using Combot.IRCServices.Messaging;
using Combot.Modules;

namespace Combot
{
    public class Bot
    {
        public event Action<CommandMessage> CommandReceivedEvent;
        public event Action<BotError> ErrorEvent;
        public ServerConfig ServerConfig;
        public IRC IRC;
        public List<Module> Modules;
        public bool Connected = false;
        public bool LoggedIn = false;
        public Dictionary<PrivilegeMode, AccessType> PrivilegeModeMapping = new Dictionary<PrivilegeMode, AccessType>() { { PrivilegeMode.v, AccessType.Voice }, { PrivilegeMode.h, AccessType.HalfOperator }, { PrivilegeMode.o, AccessType.Operator }, { PrivilegeMode.a, AccessType.SuperOperator }, { PrivilegeMode.q, AccessType.Founder } };
        public Dictionary<ChannelMode, AccessType> ChannelModeMapping = new Dictionary<ChannelMode, AccessType>() { { ChannelMode.v, AccessType.Voice }, { ChannelMode.h, AccessType.HalfOperator }, { ChannelMode.o, AccessType.Operator }, { ChannelMode.a, AccessType.SuperOperator }, { ChannelMode.q, AccessType.Founder } };

        private bool GhostSent;
        private int CurNickChoice;

        public Bot(ServerConfig serverConfig)
        {
            Modules = new List<Module>();
            GhostSent = false;
            CurNickChoice = 0;
            ServerConfig = serverConfig;

            IRC = new IRC(serverConfig.MaxMessageLength, serverConfig.MessageSendDelay);
            IRC.ConnectEvent += HandleConnectEvent;
            IRC.DisconnectEvent += HandleDisconnectEvent;
            IRC.Message.ServerReplyEvent += HandleReplyEvent;
            IRC.Message.ChannelMessageReceivedEvent += HandleChannelMessageReceivedEvent;
            IRC.Message.PrivateMessageReceivedEvent += HandlePrivateMessageReceivedEvent;
            IRC.Message.PrivateNoticeReceivedEvent += HandlePrivateNoticeReceivedEvent;
            IRC.Message.JoinChannelEvent += HandleJoinEvent;
            IRC.Message.KickEvent += HandleKickEvent;
            IRC.Message.ChannelModeChangeEvent += HandleChannelModeChangeEvent;

            LoadModules();
        }

        /// <summary>
        /// Trys to connect to one of the IPs of the given hostname.  If the connection was successful, it will login the nick.
        /// </summary>
        public void Connect()
        {
            GhostSent = false;
            CurNickChoice = 0;
            bool serverConnected = false;
            int i = 0;
            do
            {
                if (ServerConfig.Hosts.Count > i)
                {
                    IPAddress[] ipList = Dns.GetHostAddresses(ServerConfig.Hosts[i].Host);
                    foreach (IPAddress ip in ipList)
                    {
                        serverConnected = IRC.Connect(ip, ServerConfig.Hosts[i].Port);
                        if (serverConnected)
                        {
                            break;
                        }
                    }
                    i++;
                }
                else
                {
                    break;
                }
            }
            while (!serverConnected);

            if (serverConnected)
            {
                if (CurNickChoice < ServerConfig.Nicknames.Count)
                {
                    IRC.Login(ServerConfig.Name, new Nick()
                    {
                        Nickname = ServerConfig.Nicknames[CurNickChoice],
                        Host = Dns.GetHostName(),
                        Realname = ServerConfig.Realname,
                        Username = ServerConfig.Username
                    });
                }
                else
                {
                    Disconnect();
                }
            }
        }

        /// <summary>
        /// Disconnects from the current server.
        /// </summary>
        public void Disconnect()
        {
            IRC.Disconnect();
            Connected = false;
            LoggedIn = false;
        }

        public void LoadModules()
        {
            foreach (Module module in ServerConfig.Modules)
            {
                if (module.Enabled && !Modules.Exists(mod => mod.ClassName == module.ClassName))
                {
                    Module loadedModule = module.CreateInstance(this);
                    if (loadedModule.Loaded)
                    {
                        Modules.Add(loadedModule);
                    }
                }
            }
        }

        public bool CheckChannelAccess(string channel, string nickname, AccessType access)
        {
            Channel foundChannel = IRC.Channels.Find(chan => chan.Name == channel);
            if (foundChannel != null)
            {
                Nick foundNick = foundChannel.Nicks.Find(nick => nick.Nickname == nickname);
                if (foundNick != null)
                {
                    for (int i = 0; i < foundNick.Privileges.Count; i++)
                    {
                        switch (PrivilegeModeMapping[foundNick.Privileges[i]])
                        {
                            case AccessType.User:
                                if (access == AccessType.User)
                                {
                                    return true;
                                }
                                break;
                            case AccessType.Voice:
                                if (access == AccessType.User || access == AccessType.Voice)
                                {
                                    return true;
                                }
                                break;
                            case AccessType.HalfOperator:
                                if (access == AccessType.User || access == AccessType.Voice || access == AccessType.HalfOperator)
                                {
                                    return true;
                                }
                                break;
                            case AccessType.Operator:
                                if (access == AccessType.User || access == AccessType.Voice || access == AccessType.HalfOperator || access == AccessType.Operator)
                                {
                                    return true;
                                }
                                break;
                            case AccessType.SuperOperator:
                                if (access == AccessType.User || access == AccessType.Voice || access == AccessType.HalfOperator || access == AccessType.Operator || access == AccessType.SuperOperator)
                                {
                                    return true;
                                }
                                break;
                            case AccessType.Founder:
                                if (access == AccessType.User || access == AccessType.Voice || access == AccessType.HalfOperator || access == AccessType.Operator || access == AccessType.SuperOperator || access == AccessType.Founder)
                                {
                                    return true;
                                }
                                break;
                            case AccessType.Owner:
                                return true;
                                break;
                        }
                    }
                }
            }
            return false;
        }

        public bool CheckChannelAccess(string channel, string nickname, List<AccessType> access)
        {
            bool hasAccess = false;

            for (int i = 0; i < access.Count; i++)
            {
                hasAccess = CheckChannelAccess(channel, nickname, access[i]);
                if (hasAccess)
                {
                    break;
                }
            }

            return hasAccess;
        }

        private void HandleConnectEvent()
        {
            Connected = true;
        }

        private void HandleDisconnectEvent()
        {
            Connected = false;
        }

        private void HandleJoinEvent(object sender, JoinChannelInfo info)
        {
            if (info.Nick.Nickname == IRC.Nickname)
            {
                if (!ServerConfig.Channels.Exists(chan => chan.Name == info.Channel))
                {
                    ChannelConfig chanConfig = new ChannelConfig();
                    chanConfig.Name = info.Channel;
                    ServerConfig.Channels.Add(chanConfig);
                    ServerConfig.Save();
                }
            }
        }

        private void HandleKickEvent(object sender, KickInfo info)
        {
            if (info.KickedNick.Nickname == IRC.Nickname)
            {
                ServerConfig.Channels.RemoveAll(chan => chan.Name == info.Channel);
                ServerConfig.Save();
            }
        }

        private void HandleChannelModeChangeEvent(object sender, ChannelModeChangeInfo e)
        {
            ChannelConfig channel = ServerConfig.Channels.Find(chan => chan.Name == e.Channel);
            if (channel != null)
            {
                foreach (ChannelModeInfo mode in e.Modes)
                {
                    switch (mode.Mode)
                    {
                        case ChannelMode.k:
                            if (mode.Set)
                            {
                                channel.Key = mode.Parameter;
                            }
                            else
                            {
                                channel.Key = string.Empty;
                            }
                            ServerConfig.Save();
                            break;
                    }
                }
            }
        }

        private void HandleReplyEvent(object sender, IReply e)
        {
            if (e.GetType() == typeof(ServerReplyMessage))
            {
                ServerReplyMessage reply = (ServerReplyMessage)e;
                switch (reply.ReplyCode)
                {
                    case IRCReplyCode.RPL_WELCOME:
                        // If the reply is Welcome, that means we are fully connected to the server.
                        LoggedIn = true;
                        if (!GhostSent && IRC.Nickname != ServerConfig.Nicknames[CurNickChoice])
                        {
                            IRC.SendPrivateMessage("NickServ", string.Format("GHOST {0} {1}", ServerConfig.Nicknames[CurNickChoice], ServerConfig.Password));
                            Thread.Sleep(1000);
                            IRC.SendNick(ServerConfig.Nicknames[CurNickChoice]);
                            GhostSent = true;
                        }
                        // Identify to NickServ if need be
                        IRC.SendPrivateMessage("NickServ", string.Format("IDENTIFY {0}", ServerConfig.Password));

                        // Join all required channels
                        // Delay joining channels for configured time
                        Thread.Sleep(ServerConfig.JoinDelay);
                        foreach (ChannelConfig channel in ServerConfig.Channels)
                        {
                            IRC.SendJoin(channel.Name, channel.Key);
                        }
                        break;
                }
            }
            else if (e.GetType() == typeof(ServerErrorMessage))
            {
                ServerErrorMessage error = (ServerErrorMessage) e;
                switch (error.ErrorCode)
                {
                    case IRCErrorCode.ERR_NOTREGISTERED:
                        if (ServerConfig.AutoRegister && ServerConfig.Password != string.Empty && ServerConfig.Email != string.Empty)
                        {
                            IRC.SendPrivateMessage("NickServ", string.Format("REGISTER {0} {1}", ServerConfig.Password, ServerConfig.Email));
                        }
                        break;
                    case IRCErrorCode.ERR_NICKNAMEINUSE:
                        if (LoggedIn == false)
                        {
                            string nick = string.Empty;
                            if (IRC.Nickname == ServerConfig.Nicknames[CurNickChoice] && ServerConfig.Nicknames.Count > CurNickChoice + 1)
                            {
                                CurNickChoice++;
                                nick = ServerConfig.Nicknames[CurNickChoice];
                            }
                            else
                            {
                                Random rand = new Random();
                                nick = string.Format("{0}_{1}", ServerConfig.Nicknames.First(), rand.Next(100000).ToString());
                            }
                            IRC.Login(ServerConfig.Name, new Nick()
                            {
                                Nickname = nick,
                                Host = Dns.GetHostName(),
                                Realname = ServerConfig.Realname,
                                Username = ServerConfig.Username
                            });
                        }
                        break;
                }
            }
        }

        private void HandleChannelMessageReceivedEvent(object sender, ChannelMessage e)
        {
            // The message was a command
            if (e.Message.StartsWith(ServerConfig.CommandPrefix))
            {
                if (!ServerConfig.ChannelBlacklist.Contains(e.Channel)
                    && !ServerConfig.NickBlacklist.Contains(e.Sender.Nickname)
                    )
                {
                    ParseCommandMessage(e.TimeStamp, e.Message, e.Sender, e.Channel, MessageType.Channel);
                }
            }
        }

        private void HandlePrivateMessageReceivedEvent(object sender, PrivateMessage e)
        {
            // The message was a command
            if (e.Message.StartsWith(ServerConfig.CommandPrefix))
            {
                if (!ServerConfig.NickBlacklist.Contains(e.Sender.Nickname))
                {
                    ParseCommandMessage(e.TimeStamp, e.Message, e.Sender, e.Sender.Nickname, MessageType.Query);
                }
            }
        }

        private void HandlePrivateNoticeReceivedEvent(object sender, PrivateNotice e)
        {
            // The notice was a command
            if (e.Message.StartsWith(ServerConfig.CommandPrefix))
            {
                if (!ServerConfig.NickBlacklist.Contains(e.Sender.Nickname))
                {
                    ParseCommandMessage(e.TimeStamp, e.Message, e.Sender, e.Sender.Nickname, MessageType.Notice);
                }
            }
        }

        private void ParseCommandMessage(DateTime timestamp, string message, Nick sender, string location, MessageType messageType)
        {
            // Extract command and arguments
            string[] msgArgs = message.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            string command = msgArgs[0].Remove(0, ServerConfig.CommandPrefix.Length);
            List<string> argsOnly = msgArgs.ToList();
            argsOnly.RemoveAt(0);

            // Find the module that contains the command
            Module module = Modules.Find(mod => mod.Commands.Exists(c => c.Triggers.Contains(command)) && mod.Loaded);
            if (module != null)
            {
                // Find the command
                Command cmd = module.Commands.Find(c => c.Triggers.Contains(command));
                if (cmd != null)
                {
                    CommandMessage newCommand = new CommandMessage();
                    newCommand.Nick.Copy(sender);
                    bool nickFound = false;
                    IRC.Channels.ForEach(channel => channel.Nicks.ForEach(nick =>
                    {
                        if (nick.Nickname == newCommand.Nick.Nickname)
                        {
                            nickFound = true;
                            newCommand.Nick.AddModes(nick.Modes);
                            newCommand.Nick.AddPrivileges(nick.Privileges);
                        }
                    }));
                    // Nickname has not been found, so need to run a query for nick's modes
                    if (!nickFound)
                    {
                        string whoStyle = string.Format(@"[^\s]+\s[^\s]+\s[^\s]+\s[^\s]+\s({0})\s(?<Modes>[^\s]+)\s:[\d]\s(.+)", newCommand.Nick.Nickname);
                        Regex whoRegex = new Regex(whoStyle);
                        IRC.SendWho(newCommand.Nick.Nickname);
                        ServerReplyMessage whoReply = IRC.Message.GetServerReply(IRCReplyCode.RPL_WHOREPLY, whoStyle);
                        if (whoReply.ReplyCode != 0)
                        {
                            Match whoMatch = whoRegex.Match(whoReply.Message);

                            List<UserModeInfo> modeInfoList = IRC.ParseUserModeString(whoMatch.Groups["Modes"].ToString());
                            modeInfoList.ForEach(info =>
                            {
                                if (info.Set)
                                {
                                    newCommand.Nick.AddMode(info.Mode);
                                }
                            });
                        }
                    }
                    newCommand.TimeStamp = timestamp;
                    newCommand.Location = location;
                    newCommand.MessageType = messageType;
                    newCommand.Command = command;
                    
                    // Check arguments against required arguments
                    List<CommandArgument> typeArguments = cmd.Arguments.FindAll(arg => arg.MessageTypes.Contains(messageType));
                    List<CommandArgument> validArguments = new List<CommandArgument>();
                    if (argsOnly.Count > 0)
                    {
                        string[] argSplit = argsOnly.First().Split(new[] { ' ' }, typeArguments.Count, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < typeArguments.Count; i++)
                        {
                            if (argsOnly.Count > i)
                            {
                                
                            }
                        }
                        
                        int argIndex = 0;
                        for (int i = 0; i < typeArguments.Count && argIndex <= argSplit.GetUpperBound(0); i++)
                        {
                            bool allowedArg = true;
                            if (typeArguments[i].DependentArguments.Count > 0)
                            {
                                allowedArg = typeArguments[i].DependentArguments.Exists(arg =>
                                {
                                    if (newCommand.Arguments.ContainsKey(arg.Name))
                                    {
                                        string argValue = newCommand.Arguments[arg.Name];
                                        return arg.Values.Exists(val => val.ToLower() == argValue.ToLower());
                                    }
                                    return false;
                                });
                            }
                            if (allowedArg)
                            {
                                newCommand.Arguments.Add(typeArguments[i].Name, argSplit[i]);
                                CommandArgument newArgument = new CommandArgument();
                                newArgument.Copy(typeArguments[i]);
                                validArguments.Add(newArgument);
                                argIndex++;
                            }
                        }
                    }
                    bool invalidArgs = false;
                    for (int i = 0; i < newCommand.Arguments.Count; i++)
                    {
                        if (validArguments[i].AllowedValues.Count > 0)
                        {
                            // Check to see if any of the arguments are invalid
                            string argVal = newCommand.Arguments[validArguments[i].Name];
                            if (!validArguments[i].AllowedValues.Exists(val => val.ToLower() == argVal.ToLower()))
                            {
                                invalidArgs = true;
                                string invalidMessage = string.Format("Invalid value for \u0002{0}\u000F in \u0002{1}{2} {3}\u000F.  Valid options are \u0002{4}\u000F.", validArguments[i].Name, ServerConfig.CommandPrefix, command, string.Join(" ", validArguments.Select(arg => { if (arg.Required) { return "\u001F" + arg.Name + "\u000F\u0002"; } return "[\u001F" + arg.Name + "\u000F\u0002]"; })), string.Join(", ", validArguments[i].AllowedValues));
                                switch (messageType)
                                {
                                    case MessageType.Channel:
                                    case MessageType.Query:
                                        IRC.SendPrivateMessage(location, invalidMessage);
                                        break;
                                    case MessageType.Notice:
                                        IRC.SendNotice(location, invalidMessage);
                                        break;
                                }
                                break;
                            }
                        }
                    }
                    if (validArguments.FindAll(arg => arg.Required).Count <= newCommand.Arguments.Count)
                    {
                        if (!invalidArgs)
                        {
                            if (CommandReceivedEvent != null)
                            {
                                CommandReceivedEvent(newCommand);
                            }
                        }
                    }
                    else
                    {
                        string missingArgument = string.Format("Missing a required argument for \u0002{0}{1} {2}\u000F.  The required arguments are \u0002{3}\u000F.", ServerConfig.CommandPrefix, command, string.Join(" ", validArguments.Select(arg => { if (arg.Required) { return "\u001F" + arg.Name + "\u000F\u0002"; } return "[\u001F" + arg.Name + "\u000F\u0002]"; })), string.Join(", ", validArguments.Where(arg => arg.Required).Select(arg => arg.Name)));
                        if (messageType == MessageType.Channel || messageType == MessageType.Query)
                        {
                            IRC.SendPrivateMessage(location, missingArgument);
                        }
                        else if (messageType == MessageType.Notice)
                        {
                            IRC.SendNotice(location, missingArgument);
                        }
                    }
                }
            }
        }
    }
}
