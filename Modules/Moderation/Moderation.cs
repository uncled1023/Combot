﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Combot.IRCServices;
using Timer = System.Timers.Timer;

namespace Combot.Modules.Plugins
{
    public class Moderation : Module
    {
        private List<Timer> unbanTimers;
        private ReaderWriterLockSlim listLock;

        public override void Initialize()
        {
            unbanTimers = new List<Timer>();
            listLock = new ReaderWriterLockSlim();
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
                    ModifyChannelModes(foundCommand, command);
                    break;
                case "Topic":
                    ModifyChannelTopic(foundCommand, command);
                    break;
                case "Invite":
                    InviteNick(foundCommand, command);
                    break;
                case "Ban":
                    BanNick(true, foundCommand, command);
                    break;
                case "UnBan":
                    BanNick(false, foundCommand, command);
                    break;
                case "Kick Ban":
                    BanNick(true, foundCommand, command);
                    KickNick(foundCommand, command);
                    break;
                case "Timed Ban":
                    TimedBan(foundCommand, command);
                    break;
                case "Timed Kick Ban":
                    TimedBan(foundCommand, command);
                    KickNick(foundCommand, command);
                    break;
                case "Kick":
                    KickNick(foundCommand, command);
                    break;
                case "Kick Self":
                    KickSelf(command);
                    break;
                case "Clear":
                    ClearChannel(foundCommand, command);
                    break;
                case "Roll Call":
                    string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
                    Channel foundChannel = Bot.IRC.Channels.Find(chan => chan.Name == channel);
                    if (foundChannel != null)
                    {
                        string rollCall = string.Join(", ", foundChannel.Nicks.Select(nick => nick.Nickname));
                        Bot.IRC.Command.SendPrivateMessage(channel, "It's time for a Roll Call!");
                        Bot.IRC.Command.SendPrivateMessage(channel, rollCall);
                    }
                    else
                    {
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, string.Format("I am not in \u0002{0}\u0002", channel));
                    }
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
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage);
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
                Bot.IRC.Command.SendPrivateMessage("ChanServ", string.Format("{0} {1} {2} {3}", optionCommand, channel, command.Arguments["Option"], command.Arguments["Nickname"]));
            }
            else
            {
                string noAccessMessage = string.Format("You do not have access to \u0002{0}\u000F \u0002{1}\u000F to the \u0002{2}\u000F list on \u0002{3}\u000F.", command.Arguments["Option"].ToLower(), command.Arguments["Nickname"], optionCommand, channel);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage);
            }
        }

        private void ModifyChannelModes(Command curCommand, CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            bool allowedMode = true;
            bool allowedCommand = Bot.CheckChannelAccess(channel, command.Nick.Nickname, curCommand.AllowedAccess);
            if (allowedCommand)
            {
                List<ChannelModeInfo> modeList = new List<ChannelModeInfo>();
                if (command.Arguments.ContainsKey("Parameters"))
                {
                    modeList = Bot.IRC.ParseChannelModeString(command.Arguments["Modes"],
                        command.Arguments["Parameters"]);
                }
                else
                {
                    modeList = Bot.IRC.ParseChannelModeString(command.Arguments["Modes"], string.Empty);
                }
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
                    Bot.IRC.Command.SendMode(channel, modeList);
                }
                else
                {
                    string noAccessMessage = string.Format("You do not have access to set mode \u0002+{0}\u000F on \u0002{1}\u000F.", mode, channel);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage);
                }
            }
            else
            {
                string noAccessMessage = string.Format("You do not have access to use \u0002{0}\u000F on \u0002{1}\u000F.", command.Command, channel);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage);
            }
        }

        private void SetMode(bool set, string channel, ChannelMode mode, string nickname)
        {
            ChannelModeInfo modeInfo = new ChannelModeInfo();
            modeInfo.Mode = mode;
            modeInfo.Parameter = nickname;
            modeInfo.Set = set;
            Bot.IRC.Command.SendMode(channel, modeInfo);
        }

        private void ModifyChannelTopic(Command curCommand, CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            if (Bot.CheckChannelAccess(channel, command.Nick.Nickname, curCommand.AllowedAccess))
            {
                Bot.IRC.Command.SendTopic(channel, command.Arguments["Message"]);
            }
            else
            {
                string noAccessMessage = string.Format("You do not have access to change the topic on \u0002{0}\u000F.", channel);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage);
            }
        }

        private void InviteNick(Command curCommand, CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            if (Bot.CheckChannelAccess(channel, command.Nick.Nickname, curCommand.AllowedAccess))
            {
                Bot.IRC.Command.SendInvite(channel, command.Arguments["Nickname"]);
            }
            else
            {
                string noAccessMessage = string.Format("You do not have access to invite someone to \u0002{0}\u000F.", channel);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage);
            }
        }

        private void BanNick(bool set, Command curCommand, CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            if (Bot.CheckChannelAccess(channel, command.Nick.Nickname, curCommand.AllowedAccess) && Bot.CheckNickAccess(channel, command.Nick.Nickname, command.Arguments["Nickname"]))
            {
                string banMask = command.Arguments["Nickname"];
                Channel foundChannel = Bot.IRC.Channels.Find(chan => chan.Nicks.Exists(nick => nick.Nickname == banMask));
                if (foundChannel != null)
                {
                    Nick foundNick = foundChannel.Nicks.Find(nick => nick.Nickname == banMask);
                    if (foundNick.Host != string.Empty && foundNick.Username != null)
                    {
                        banMask = string.Format("{0}!*{1}@{2}", foundNick.Nickname, foundNick.Username, foundNick.Host);
                    }
                    else if (foundNick.Host != string.Empty)
                    {
                        banMask = string.Format("{0}!*@{1}", foundNick.Nickname, foundNick.Host);
                    }
                    else if (foundNick.Username != string.Empty)
                    {
                        banMask = string.Format("{0}!*{1}@*", foundNick.Nickname, foundNick.Username);
                    }
                    else
                    {
                        banMask = string.Format("{0}!*@*", foundNick.Nickname);
                    }
                }
                else
                {
                    if (!banMask.Contains("@") || !banMask.Contains("!"))
                    {
                        banMask = string.Format("{0}!*@*", banMask);
                    }
                }
                SetMode(set, channel, ChannelMode.b, banMask);
            }
            else
            {
                string banMessage = "ban";
                if (!set)
                {
                    banMessage = "unban";
                }
                string noAccessMessage = string.Format("You do not have access to {0} \u0002{1}\u000F on \u0002{2}\u000F.", banMessage, command.Arguments["Nickname"], channel);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage);
            }
        }

        private void TimedBan(Command curCommand, CommandMessage command)
        {
            double timeout;
            if (double.TryParse(command.Arguments["Time"], out timeout))
            {
                BanNick(true, curCommand, command);
                Timer unban_trigger = new Timer();
                unban_trigger.Interval = (timeout * 1000.0);
                unban_trigger.Enabled = true;
                unban_trigger.AutoReset = false;
                unban_trigger.Elapsed += (sender, e) => TimedUnBan(sender, e, curCommand, command);
                listLock.EnterWriteLock();
                unbanTimers.Add(unban_trigger);
                listLock.ExitWriteLock();
            }
            else
            {
                string notValid = "Please enter a valid time.";
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, notValid);
            }
        }

        private void TimedUnBan(object sender, EventArgs e, Command curCommand, CommandMessage command)
        {
            Timer unbanTimer = (Timer)sender;
            unbanTimer.Enabled = false;
            BanNick(false, curCommand, command);
            listLock.EnterWriteLock();
            unbanTimers.Remove(unbanTimer);
            listLock.ExitWriteLock();
        }


        private void TimedUnvoice(Command curCommand, CommandMessage command)
        {
            double timeout;
            if (double.TryParse(command.Arguments["Time"], out timeout))
            {
                ModifyUserPrivilege(false, command, ChannelMode.v);
                Timer revoice_trigger = new Timer();
                revoice_trigger.Interval = (timeout * 1000.0);
                revoice_trigger.Enabled = true;
                revoice_trigger.AutoReset = false;
                revoice_trigger.Elapsed += (sender, e) => TimedRevoice(sender, e, curCommand, command);
                listLock.EnterWriteLock();
                revoiceTimers.Add(revoice_trigger);
                listLock.ExitWriteLock();
            }
            else
            {
                string notValid = "Please enter a valid time.";
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, notValid);
            }
        }

        private void TimedRevoice(object sender, EventArgs e, Command curCommand, CommandMessage command)
        {
            Timer revoiceTimer = (Timer)sender;
            revoiceTimer.Enabled = false;
            ModifyUserPrivilege(true, command, ChannelMode.v);
            listLock.EnterWriteLock();
            revoiceTimers.Remove(revoiceTimer);
            listLock.ExitWriteLock();
        }

        private void KickNick(Command curCommand, CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            if (Bot.CheckChannelAccess(channel, command.Nick.Nickname, curCommand.AllowedAccess) && Bot.CheckNickAccess(channel, command.Nick.Nickname, command.Arguments["Nickname"]))
            {
                if (command.Arguments.ContainsKey("Reason"))
                {
                    Bot.IRC.Command.SendKick(channel, command.Arguments["Nickname"], command.Arguments["Reason"]);
                }
                else
                {
                    Bot.IRC.Command.SendKick(channel, command.Arguments["Nickname"]);
                }
            }
            else
            {
                string noAccessMessage = string.Format("You do not have access to kick \u0002{0}\u000F from \u0002{1}\u000F.", command.Arguments["Nickname"], channel);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage);
            }
        }

        private void KickSelf(CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            if (command.Arguments.ContainsKey("Reason"))
            {
                Bot.IRC.Command.SendKick(channel, command.Nick.Nickname, command.Arguments["Reason"]);
            }
            else
            {
                Bot.IRC.Command.SendKick(channel, command.Nick.Nickname);
            }
        }

        private void ClearChannel(Command curCommand, CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            if (Bot.CheckChannelAccess(channel, command.Nick.Nickname, curCommand.AllowedAccess))
            {
                Bot.IRC.Command.SendPrivateMessage("ChanServ", string.Format("CLEAR {0} {1}", channel, command.Arguments["Target"]));
            }
            else
            {
                string noAccessMessage = string.Format("You do not have access to clear \u0002{0}\u000F on \u0002{1}\u000F.", command.Arguments["Target"], channel);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage);
            }
        }
    }
}
