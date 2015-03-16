using System;
using System.Collections.Generic;
using System.Linq;
using Combot.Databases;

namespace Combot.Modules.Plugins
{
    public class Channel_Rules : Module
    {
        public override void Initialize()
        {
            Bot.CommandReceivedEvent += HandleCommandEvent;
        }

        public override void ParseCommand(CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            Command foundCommand = Commands.Find(c => c.Triggers.Contains(command.Command));
            switch (foundCommand.Name)
            {
                case "Rules Display":
                    List<Dictionary<string, object>> foundRules = GetRuleList(channel);
                    if (foundRules.Any())
                    {
                        int index = 1;
                        foundRules.ForEach(rule =>
                        {
                            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, string.Format("Rule \u0002#{0}\u0002: {1}", index, rule["rule"]));
                            index++;
                        });
                    }
                    else
                    {
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, string.Format("There are no rules for \u0002{0}\u0002", channel));
                    }
                    break;
                case "Rules Modification":
                    if (Bot.CheckChannelAccess(channel, command.Nick.Nickname, command.Access))
                    {
                        string action = command.Arguments["Action"].ToString();
                        switch (action.ToLower())
                        {
                            case "add":
                                AddRule(command);
                                break;
                            case "edit":
                                EditRule(command);
                                break;
                            case "del":
                                DeleteRule(command);
                                break;
                        }
                    }
                    else
                    {
                        string noAccessMessage = string.Format("You do not have access to \u0002{0}\u000F on \u0002{1}\u000F.", command.Command, channel);
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, noAccessMessage);
                    }
                    break;
            }
        }

        private void AddRule(CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            List<Dictionary<string, object>> results = GetRuleList(channel);

            AddChannel(channel);
            string query = "INSERT INTO `channelrules` SET " +
                            "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                            "`channel_id` = (SELECT `channels`.`id` FROM `channels` INNER JOIN `servers` ON `servers`.`id` = `channels`.`server_id` WHERE `servers`.`name` = {1} && `channels`.`name` = {2}), " +
                            "`rule` = {3}, " +
                            "`date_added` = {4}";
            Bot.Database.Execute(query, new object[] { Bot.ServerConfig.Name, Bot.ServerConfig.Name, channel, command.Arguments["Rule"], command.TimeStamp });
            string ruleMessage = string.Format("Rule Added. \u0002{0}\u0002 now has \u0002{1}\u0002 rules.", channel, results.Count + 1);
            SendResponse(command.MessageType, command.Location, command.Nick.Nickname, ruleMessage);
        }

        private void EditRule(CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            List<Dictionary<string, object>> results = GetRuleList(channel);
            int num = 0;
            if (int.TryParse(command.Arguments["ID"], out num))
            {
                if (results.Count >= num)
                {
                    int id = Convert.ToInt32(results[num - 1]["id"]);
                    string query = "UPDATE `channelrules` SET " +
                                   "`rule` = {0} " +
                                   "WHERE `id` = {1}";
                    Bot.Database.Execute(query, new object[] { command.Arguments["Rule"], id });
                    string ruleMessage = string.Format("Rule \u0002#{0}\u0002 for \u0002{1}\u0002 is now: {2}", num, channel, command.Arguments["Rule"]);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, ruleMessage);
                }
                else
                {
                    string invalid = "Invalid Rule ID.";
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid);
                }
            }
            else
            {
                string invalid = "Invalid Rule ID.";
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid);
            }
        }

        private void DeleteRule(CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            List<Dictionary<string, object>> results = GetRuleList(channel);
            int num = 0;
            if (int.TryParse(command.Arguments["ID"], out num))
            {
                if (results.Count >= num)
                {
                    int id = Convert.ToInt32(results[num - 1]["id"]);
                    string query = "DELETE FROM `channelrules` " +
                                   "WHERE `id` = {0}";
                    Bot.Database.Execute(query, new object[] { id });
                    string ruleMessage = string.Format("Rule \u0002#{0}\u0002 for \u0002{1}\u0002 has been deleted.", num, channel);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, ruleMessage);
                }
                else
                {
                    string invalid = "Invalid Rule ID.";
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid);
                }
            }
            else
            {
                string invalid = "Invalid Rule ID.";
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid);
            }
        }

        private List<Dictionary<string, object>> GetRuleList(string channel)
        {
            string search = "SELECT `channelrules`.`id`, `channelrules`.`rule` FROM `channelrules` " +
                            "INNER JOIN `channels` " +
                            "ON `channelrules`.`channel_id` = `channels`.`id` " +
                            "INNER JOIN `servers` " +
                            "ON `channelrules`.`server_id` = `servers`.`id` " +
                            "WHERE `servers`.`name` = {0} AND `channels`.`name` = {1} " +
                            "ORDER BY date_added ASC";
            return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, channel });
        }
    }
}
