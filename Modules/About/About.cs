using System;
using System.Reflection;

namespace Combot.Modules.Plugins
{
    public class About : Module
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
                case "About":
                    string ownerNum = " is";
                    if (Bot.ServerConfig.Owners.Count > 1)
                    {
                        ownerNum = "s are";
                    }
                    string aboutMessage = string.Format("Combot v{0} is created by Uncled1023.  My current owner{1} {2}.", Assembly.GetExecutingAssembly().GetName().Version, ownerNum, string.Join(", ", Bot.ServerConfig.Owners));
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, aboutMessage);
                    break;
                case "Source":
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, string.Format("You can find my source code here: {0}", GetOptionValue("Source Code")));
                    break;
                case "Uptime":
                    DateTime now = DateTime.Now;
                    int days = now.Subtract(Bot.ConnectionTime).Days;
                    int hours = now.Subtract(Bot.ConnectionTime).Hours;
                    int minutes = now.Subtract(Bot.ConnectionTime).Minutes;
                    int seconds = now.Subtract(Bot.ConnectionTime).Seconds;
                    string uptime = string.Empty;

                    if (days > 0)
                    {
                        string plural = (days > 1) ? "s" : string.Empty;
                        uptime += string.Format("{0} day{1}, ", days, plural);
                    }
                    if (hours > 0)
                    {
                        string plural = (hours > 1) ? "s" : string.Empty;
                        uptime += string.Format("{0} hour{1}, ", hours, plural);
                    }
                    if (minutes > 0)
                    {
                        string plural = (minutes > 1) ? "s" : string.Empty;
                        uptime += string.Format("{0} minute{1}, ", minutes, plural);
                    }
                    if (seconds > 0)
                    {
                        string plural = (seconds > 1) ? "s" : string.Empty;
                        uptime += string.Format("{0} second{1}", seconds, plural);
                    }
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, string.Format("I have been connected to this server for \u0002{0}\u0002.", uptime.Trim().TrimEnd(',')));
                    break;
                case "Runtime":
                    DateTime runtimeNow = DateTime.Now;
                    int totalDays = runtimeNow.Subtract(Bot.LoadTime).Days;
                    int totalHours = runtimeNow.Subtract(Bot.LoadTime).Hours;
                    int totalMinutes = runtimeNow.Subtract(Bot.LoadTime).Minutes;
                    int totalSeconds = runtimeNow.Subtract(Bot.LoadTime).Seconds;
                    string runtime = string.Empty;

                    if (totalDays > 0)
                    {
                        string plural = (totalDays > 1) ? "s" : string.Empty;
                        runtime += string.Format("{0} day{1}, ", totalDays, plural);
                    }
                    if (totalHours > 0)
                    {
                        string plural = (totalHours > 1) ? "s" : string.Empty;
                        runtime += string.Format("{0} hour{1}, ", totalHours, plural);
                    }
                    if (totalMinutes > 0)
                    {
                        string plural = (totalMinutes > 1) ? "s" : string.Empty;
                        runtime += string.Format("{0} minute{1}, ", totalMinutes, plural);
                    }
                    if (totalSeconds > 0)
                    {
                        string plural = (totalSeconds > 1) ? "s" : string.Empty;
                        runtime += string.Format("{0} second{1}", totalSeconds, plural);
                    }
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, string.Format("I have been running for \u0002{0}\u0002.", runtime.Trim().TrimEnd(',')));
                    break;

            }
        }
    }
}
