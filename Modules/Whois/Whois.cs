using System.Collections.Generic;
using System.Linq;

namespace Combot.Modules.Plugins
{
    public class Whois : Module
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
                case "Whois":
                    string option = (command.Arguments.ContainsKey("Option")) ? command.Arguments["Option"] : string.Empty;
                    string mask = (command.Arguments.ContainsKey("Nickname")) ? command.Arguments["Nickname"] : command.Nick.Nickname;
                    List<string> nicksList = new List<string>();
                    List<string> hostList = new List<string>();
                    findMatches(ref nicksList, ref hostList, mask, mask);
                    if (nicksList.Any() || hostList.Any())
                    {
                        // display results
                        if (nicksList.Any() && (string.IsNullOrEmpty(option) || option.ToLower() == "nicks"))
                        {
                            string nicksFound = string.Format("\u0002{0}\u0002 has been seen as: \u0002{1}\u0002", mask, string.Join(", ", nicksList));
                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, nicksFound, true);
                        }
                        if (hostList.Any() && (string.IsNullOrEmpty(option) || option.ToLower() == "hosts"))
                        {
                            string hostsFound = string.Format("\u0002{0}\u0002 has used the following hosts: \u0002{1}\u0002", mask, string.Join(", ", hostList));
                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, hostsFound, true);
                        }
                    }
                    else
                    {
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, string.Format("I have no information about \u0002{0}\u0002", mask), true);
                    }
                    break;
            }
        }

        private void findMatches(ref List<string> nickList, ref List<string> hostList, string nick, string host)
        {
            List<Dictionary<string, object>> results = findAssociationList(nick, host);
            for (int i = 0; i < results.Count; i++)
            {
                string foundNick = results[i]["nickname"].ToString();
                string foundHost = results[i]["host"].ToString();
                if (nickList != null && !nickList.Contains(foundNick))
                {
                    nickList.Add(foundNick);
                    findMatches(ref nickList, ref hostList, foundNick, foundNick);
                }
                if (hostList != null && !hostList.Contains(foundHost))
                {
                    hostList.Add(foundHost);
                    findMatches(ref nickList, ref hostList, foundHost, foundHost);
                }
            }
        }

        private List<Dictionary<string, object>> findAssociationList(string nick, string host)
        {
            string search = "SELECT `nickinfo`.`host`, `nicks`.`nickname` FROM `nickinfo` " +
                                    "INNER JOIN `nicks` " +
                                    "ON `nickinfo`.`nick_id` = `nicks`.`id` " +
                                    "INNER JOIN `servers` " +
                                    "ON `nicks`.`server_id` = `servers`.`id` " +
                                    "WHERE `servers`.`name` = {0} AND (`nicks`.`nickname` = {1} OR `nickinfo`.`host` = {2})" +
                                    "GROUP BY `nickinfo`.`host`, `nicks`.`nickname`";
            return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, nick, host });
        }
    }
}
