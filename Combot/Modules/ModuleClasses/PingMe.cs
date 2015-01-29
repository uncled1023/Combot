using System;
using System.Collections.Generic;
using Combot.IRCServices;
using Combot.IRCServices.Messaging;

namespace Combot.Modules.ModuleClasses
{
    public class PingMe : Module
    {
        private List<Dictionary<Nick, DateTime>> pingList;
 
        public override void Initialize()
        {
            pingList = new List<Dictionary<Nick, DateTime>>();
            Bot.IRC.Message.CTCPMessageRecievedEvent += HandlePingResponse;
            Bot.CommandReceivedEvent += HandleCommandEvent;
        }

        public override void HandleCommandEvent(CommandMessage command)
        {

        }

        private void HandlePingResponse(object sender, CTCPMessage e)
        {
            
        }
    }
}