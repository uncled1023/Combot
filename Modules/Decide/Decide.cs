using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Combot.Modules.Plugins
{
    public class Decide : Module
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
                case "Decide":
                    string options = command.Arguments["Options"].ToString();
                    List<string> optionList = options.Split(new[] {" or "}, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (optionList.Count > 1)
                    {
                        Random rand = new Random();
                        int choiceIndex = rand.Next(0, optionList.Count);
                        JArray prefixes = (JArray)GetOptionValue("Choice Prefixes");
                        int prefixIndex = rand.Next(0, prefixes.Count);
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, string.Format("{0} {1}", prefixes[prefixIndex], optionList[choiceIndex]));
                    }
                    else
                    {
                        Random rand = new Random();
                        JArray answers = (JArray)GetOptionValue("Choice Answers");
                        int answerIndex = rand.Next(0, answers.Count);
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, string.Format("{0}", answers[answerIndex]));
                    }
                    break;
            }
        }
    }
}
