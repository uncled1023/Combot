using System;

namespace Combot.Modules.Plugins
{
    public class Fun : Module
    {
        public override void Initialize()
        {
            Bot.CommandReceivedEvent += HandleCommandEvent;
        }

        public override void ParseCommand(CommandMessage command)
        {
            Command foundCommand = Commands.Find(c => c.Triggers.Contains(command.Command));
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            switch (foundCommand.Name)
            {
                case "Love":
                    Random random = new Random();
                    int randNum = random.Next(0, 3);
                    switch (randNum)
                    {
                        case 0:
                            Bot.IRC.SendCTCPMessage(channel, "ACTION", string.Format("gently makes love to {0}", command.Arguments["Nickname"]));
                            break;
                        case 1:
                            Bot.IRC.SendCTCPMessage(channel, "ACTION", string.Format("sings a love ballad to {0}", command.Arguments["Nickname"]));
                            break;
                        case 2:
                            Bot.IRC.SendCTCPMessage(channel, "ACTION", string.Format("slowly sneaks up behind {0}", command.Arguments["Nickname"]));
                            Bot.IRC.SendCTCPMessage(channel, "ACTION", string.Format("squeezes {0} tightly", command.Arguments["Nickname"]));
                            break;
                        case 3:
                            Bot.IRC.SendPrivateMessage(channel, string.Format("I love you {0}!  Sooo much!", command.Arguments["Nickname"]));
                            break;
                    }
                    break;
                case "Hug":
                    Bot.IRC.SendCTCPMessage(channel, "ACTION", string.Format("hugs {0}", command.Arguments["Nickname"]));
                    break;
                case "Slap":
                    Bot.IRC.SendCTCPMessage(channel, "ACTION", string.Format("slaps {0} with a large trout", command.Arguments["Nickname"]));
                    break;
                case "Brazilian Laugh":
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, "HUEHUEHUE");
                    break;
                case ".NET":
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, "Sure is enterprise quality in here.");
                    break;
                case "Bot Response":
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, GetOptionValue("Response"));
                    break;
            }
        }
    }
}
