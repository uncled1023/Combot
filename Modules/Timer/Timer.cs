using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Combot.Modules.Plugins
{
    public class Timer : Module
    {
        private List<System.Timers.Timer> timers;
        private ReaderWriterLockSlim listLock;
 
        public override void Initialize()
        {
            timers = new List<System.Timers.Timer>();
            listLock = new ReaderWriterLockSlim();
            Bot.CommandReceivedEvent += HandleCommandEvent;
        }

        public override void ParseCommand(CommandMessage command)
        {
            Command foundCommand = Commands.Find(c => c.Triggers.Contains(command.Command));
            switch (foundCommand.Name)
            {
                case "Timer":
                    double timeout;
                    string message = command.Arguments.ContainsKey("Message") ? command.Arguments["Message"] : string.Empty;
                    if (double.TryParse(command.Arguments["Time"], out timeout) && timeout > 0)
                    {
                        if (message.StartsWith(Bot.ServerConfig.CommandPrefix))
                        {
                            string cmd = message.Split(new [] { ' ' }, StringSplitOptions.RemoveEmptyEntries).First();
                            if (foundCommand.Triggers.Contains(cmd.TrimStart(Bot.ServerConfig.CommandPrefix.ToCharArray())))
                            {
                                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, "Recursion is bad.", true);
                                break;
                            }
                        }
                        System.Timers.Timer newTimer = new System.Timers.Timer();
                        newTimer.Interval = (timeout * 1000.0);
                        newTimer.Enabled = true;
                        newTimer.AutoReset = false;
                        newTimer.Elapsed += (sender, e) => TimerElapsed(sender, e, message, command);
                        listLock.EnterWriteLock();
                        timers.Add(newTimer);
                        listLock.ExitWriteLock();
                        string addedTimer = string.Format("Timer added for {0} seconds from now.", timeout);
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, addedTimer);
                    }
                    else
                    {
                        string notValid = "Please enter a valid time.";
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, notValid, true);
                    }
                    break;
            }
        }

        private void TimerElapsed(object sender, EventArgs e, string message, CommandMessage command)
        {
            System.Timers.Timer timer = (System.Timers.Timer) sender;
            timer.Enabled = false;
            listLock.EnterWriteLock();
            timers.Remove(timer);
            listLock.ExitWriteLock();
            if (message.StartsWith(Bot.ServerConfig.CommandPrefix))
            {
                Bot.ExecuteCommand(message, command.Location, command.MessageType, command.Nick);
            }
            else
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = "Your timer has elapsed!";
                }
                message = "\u0002RING RING RING\u0002 " + message;
                SendResponse(MessageType.Query, command.Location, command.Nick.Nickname, message, false);
            }
        }
    }
}
