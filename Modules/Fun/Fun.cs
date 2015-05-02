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
            string nickname = command.Arguments.ContainsKey("Nickname") ? command.Arguments["Nickname"] : command.Nick.Nickname;
            switch (foundCommand.Name)
            {
                case "Love":
                    Random random = new Random();
                    int randNum = random.Next(0, 4);
                    switch (randNum)
                    {
                        case 0:
                            Bot.IRC.Command.SendCTCPMessage(channel, "ACTION", string.Format("gently makes love to {0}", nickname));
                            break;
                        case 1:
                            Bot.IRC.Command.SendCTCPMessage(channel, "ACTION", string.Format("sings a love ballad to {0}", nickname));
                            break;
                        case 2:
                            Bot.IRC.Command.SendCTCPMessage(channel, "ACTION", string.Format("slowly sneaks up behind {0}", nickname));
                            Bot.IRC.Command.SendCTCPMessage(channel, "ACTION", string.Format("squeezes {0} tightly", nickname));
                            break;
                        case 3:
                            Bot.IRC.Command.SendPrivateMessage(channel, string.Format("I love you {0}!  Sooo much!", nickname));
                            break;
                    }
                    break;
                case "Hug":
                    Bot.IRC.Command.SendCTCPMessage(channel, "ACTION", string.Format("hugs {0}", nickname));
                    break;
                case "Slap":
                    Bot.IRC.Command.SendCTCPMessage(channel, "ACTION", string.Format("slaps {0} with a large trout", nickname));
                    break;
                case "Brazilian Laugh":
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, "HUEHUEHUE");
                    break;
                case ".NET":
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, "Sure is enterprise quality in here.");
                    break;
                case "Bot Response":
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, GetOptionValue("Response").ToString());
                    break;
            }
        }
    }
}
