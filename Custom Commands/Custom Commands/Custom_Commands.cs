using Combot.IRCServices;

namespace Combot.Modules.Plugins
{
    public class Custom_Commands : Module
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
                case "Custom Command":
                    string action = command.Arguments["Action"];
                    switch (action.ToLower())
                    {
                        case "add":
                            string addTrigger = command.Arguments["Trigger"];
                            string addResponse = command.Arguments["Response"];
                            AddCommand(command.Nick, addTrigger, addResponse);
                            break;
                        case "del":
                            string delTrigger = command.Arguments["Trigger"];
                            DeleteCommand(command.Nick, delTrigger);
                            break;
                        case "edit":
                            string editTrigger = command.Arguments["Trigger"];
                            string editResponse = command.Arguments["Response"];
                            EditCommand(command.Nick, editTrigger, editResponse);
                            break;
                        case "view":
                            if (command.Arguments.ContainsKey("Trigger"))
                            {
                                ViewTrigger(command.Nick, command.Arguments["trigger"]);
                            }
                            else
                            {
                                ViewTriggers(command.Nick);
                            }
                            break;
                    }
                    break;
                default:
                    //todo add check for custom commands here
                    break;
            }
        }

        private void AddCommand(Nick caller, string trigger, string response)
        {
            
        }

        private void DeleteCommand(Nick caller, string trigger)
        {
            
        }

        private void EditCommand(Nick caller, string trigger, string response)
        {

        }

        private void ViewTriggers(Nick caller)
        {
            
        }

        private void ViewTrigger(Nick caller, string trigger)
        {
            
        }
    }
}
