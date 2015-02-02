using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Combot.IRCServices;
using Combot.IRCServices.Messaging;

namespace Combot.Modules.ModuleClasses
{
    public class Moderation : Module
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
                // Privilege Mode Commands
                case "Founder":
                    ModifyUserPrivilege(true, command, ChannelMode.q);
                    break;
                case "Remove Founder":
                    ModifyUserPrivilege(false, command, ChannelMode.q);
                    break;
                case "SOP":
                    ModifyUserPrivilege(true, command, ChannelMode.a);
                    break;
                case "Remove SOP":
                    ModifyUserPrivilege(false, command, ChannelMode.a);
                    break;
                case "OP":
                    ModifyUserPrivilege(true, command, ChannelMode.o);
                    break;
                case "Remove OP":
                    ModifyUserPrivilege(false, command, ChannelMode.o);
                    break;
                case "HOP":
                    ModifyUserPrivilege(true, command, ChannelMode.h);
                    break;
                case "Remove HOP":
                    ModifyUserPrivilege(false, command, ChannelMode.h);
                    break;
                case "Voice":
                    ModifyUserPrivilege(true, command, ChannelMode.v);
                    break;
                case "Remove Voice":
                    ModifyUserPrivilege(false, command, ChannelMode.v);
                    break;
                // Auto Privilege Management
                case "ASOP":
                    ModifyAutoUserPrivilege("SOP", command, ChannelMode.a);
                    break;
                case "AOP":
                    ModifyAutoUserPrivilege("AOP", command, ChannelMode.o);
                    break;
                case "AHOP":
                    ModifyAutoUserPrivilege("HOP", command, ChannelMode.h);
                    break;
                case "AVoice":
                    ModifyAutoUserPrivilege("VOP", command, ChannelMode.v);
                    break;
                case "Mode":
                    ModifyChannelModes(command);
                    break;
                case "Topic":
                    ModifyChannelTopic(command);
                    break;
                case "Invite":
                    break;
                case "Ban":
                    break;
                case "UnBan":
                    break;
                case "Clear Ban":
                    break;
                case "Kick Ban":
                    break;
                case "Timed Ban":
                    break;
                case "Timed Kick Ban":
                    break;
                case "Kick":
                    break;
                case "Kick Me":
                    break;
            }
        }

        private void ModifyUserPrivilege(bool set, CommandMessage command, ChannelMode mode)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            if (Bot.CheckChannelAccess(channel, command.Nick.Nickname, Bot.ChannelModeMapping[mode]))
            {
                SetMode(set, channel, mode, command.Arguments["Nickname"]);
            }
            else
            {
                string noAccessMessage = string.Format("You do not have access to set mode \u0002+{0}\u000F for \u0002{1}\u000F on \u0002{2}\u000F.", mode, command.Arguments["Nickname"], channel);
                switch (command.MessageType)
                {
                    case MessageType.Channel:
                        Bot.IRC.SendPrivateMessage(command.Location, noAccessMessage);
                        break;
                    case MessageType.Query:
                        Bot.IRC.SendPrivateMessage(command.Nick.Nickname, noAccessMessage);
                        break;
                    case MessageType.Notice:
                        Bot.IRC.SendNotice(command.Nick.Nickname, noAccessMessage);
                        break;
                }
            }
        }

        private void ModifyAutoUserPrivilege(string optionCommand, CommandMessage command, ChannelMode mode)
        {
            bool set = true;
            if (command.Arguments["Option"].ToLower() == "del")
            {
                set = false;
            }
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            if (Bot.CheckChannelAccess(channel, command.Nick.Nickname, Bot.ChannelModeMapping[mode]))
            {
                SetMode(set, channel, mode, command.Arguments["Nickname"]);
                Bot.IRC.SendPrivateMessage("ChanServ", string.Format("{0} {1} {2} {3}", optionCommand, channel, command.Arguments["Option"], command.Arguments["Nickname"]));
            }
            else
            {
                string noAccessMessage = string.Format("You do not have access to \u0002{0}\u000F \u0002{1}\u000F to the \u0002{2}\u000F list on \u0002{3}\u000F.", command.Arguments["Option"].ToLower(), command.Arguments["Nickname"], optionCommand, channel);
                switch (command.MessageType)
                {
                    case MessageType.Channel:
                        Bot.IRC.SendPrivateMessage(command.Location, noAccessMessage);
                        break;
                    case MessageType.Query:
                        Bot.IRC.SendPrivateMessage(command.Nick.Nickname, noAccessMessage);
                        break;
                    case MessageType.Notice:
                        Bot.IRC.SendNotice(command.Nick.Nickname, noAccessMessage);
                        break;
                }
            }
        }

        private void ModifyChannelModes(CommandMessage command)
        {
            List<ChannelModeInfo> modeList = new List<ChannelModeInfo>();
            if (command.Arguments.ContainsKey("Parameters"))
            {
                modeList = Bot.IRC.ParseChannelModeString(command.Arguments["Modes"], command.Arguments["Parameters"]);
            }
            else
            {
                modeList = Bot.IRC.ParseChannelModeString(command.Arguments["Modes"], string.Empty);
            }
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            bool allowedMode = true;
            ChannelMode mode = ChannelMode.q;
            for (int i = 0; i < modeList.Count; i++)
            {
                switch (modeList[i].Mode)
                {
                    case ChannelMode.v:
                    case ChannelMode.h:
                    case ChannelMode.o:
                    case ChannelMode.a:
                    case ChannelMode.q:
                        allowedMode = Bot.CheckChannelAccess(channel, command.Nick.Nickname, Bot.ChannelModeMapping[modeList[i].Mode]);
                        if (!allowedMode)
                        {
                            mode = modeList[i].Mode;
                        }
                        break;
                }
            }
            if (allowedMode)
            {
                Bot.IRC.SendMode(channel, modeList);
            }
            else
            {
                string noAccessMessage = string.Format("You do not have access to set mode \u0002+{0}\u000F on \u0002{1}\u000F.", mode, channel);
                switch (command.MessageType)
                {
                    case MessageType.Channel:
                        Bot.IRC.SendPrivateMessage(command.Location, noAccessMessage);
                        break;
                    case MessageType.Query:
                        Bot.IRC.SendPrivateMessage(command.Nick.Nickname, noAccessMessage);
                        break;
                    case MessageType.Notice:
                        Bot.IRC.SendNotice(command.Nick.Nickname, noAccessMessage);
                        break;
                }
            }
        }

        private void SetMode(bool set, string channel, ChannelMode mode, string nickname)
        {
            ChannelModeInfo modeInfo = new ChannelModeInfo();
            modeInfo.Mode = mode;
            modeInfo.Parameter = nickname;
            modeInfo.Set = set;
            Bot.IRC.SendMode(channel, modeInfo);
        }

        private void ModifyChannelTopic(CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            if (Bot.CheckChannelAccess(channel, command.Nick.Nickname, command.Access))
            {
                Bot.IRC.SendTopic(channel, command.Arguments["Message"]);
            }
            else
            {
                string noAccessMessage = string.Format("You do not have access to change the topic on \u0002{0}\u000F.", channel);
                switch (command.MessageType)
                {
                    case MessageType.Channel:
                        Bot.IRC.SendPrivateMessage(command.Location, noAccessMessage);
                        break;
                    case MessageType.Query:
                        Bot.IRC.SendPrivateMessage(command.Nick.Nickname, noAccessMessage);
                        break;
                    case MessageType.Notice:
                        Bot.IRC.SendNotice(command.Nick.Nickname, noAccessMessage);
                        break;
                }
            }
        }
    }
}