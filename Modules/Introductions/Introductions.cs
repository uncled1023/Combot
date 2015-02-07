using System;
using System.Collections.Generic;
using System.Linq;
using Combot.Databases;
using Combot.IRCServices.Messaging;

namespace Combot.Modules.Plugins
{
    public class Introductions : Module
    {
        public override void Initialize()
        {
            Bot.CommandReceivedEvent += HandleCommandEvent;
            Bot.IRC.Message.JoinChannelEvent += HandleJoinEvent;
        }

        public override void ParseCommand(CommandMessage command)
        {
            Command foundCommand = Commands.Find(c => c.Triggers.Contains(command.Command));
            switch (foundCommand.Name)
            {
                case "Introduction":
                    string method = command.Arguments["Method"];
                    switch (method.ToLower())
                    {
                        case "add":
                            AddIntroduction(command);
                            break;
                        case "edit":
                            break;
                        case "delete":
                            break;
                        case "view":
                            break;
                    }
                    break;
            }
        }

        private void HandleJoinEvent(object sender, JoinChannelInfo info)
        {
            
        }

        private void AddIntroduction(CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            Database database = new Database(Bot.ServerConfig.Database);

            // Check to see if they have reached the max number of introductions
            string search = "SELECT `introductions`.`id` FROM `introductions` INNER JOIN `nicks` " +
                            "ON `introductions`.`nick_id` = `nicks`.`id` " +
                            "INNER JOIN `channels` " +
                            "ON `introductions`.`channel_id` = `channels`.`id` " +
                            "INNER JOIN `servers` " +
                            "ON `nicks`.`server_id` = `servers`.`id` " +
                            "WHERE `servers`.`name` = {0} AND `channels`.`name` = {1} AND `nicks`.`nickname` = {2}";
            List<Dictionary<string, object>> results = database.Query(search, new object[] { Bot.ServerConfig.Name, channel, command.Nick.Nickname });

            if (results.Count < Convert.ToInt32(GetOptionValue("Max Introductions")))
            {

                AddChannel(channel);
                AddNick(command.Nick.Nickname);
                string query = "INSERT INTO `introductions` SET " +
                               "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                               "`channel_id` = (SELECT `channels`.`id` FROM `channels` INNER JOIN `servers` ON `servers`.`id` = `channels`.`server_id` WHERE `servers`.`name` = {1} && `channels`.`name` = {2}), " +
                               "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {3} && `nickname` = {4}), " +
                               "`message` = {5}, " +
                               "`date_added` = {6}";
                database.Execute(query, new object[] {Bot.ServerConfig.Name, Bot.ServerConfig.Name, channel, Bot.ServerConfig.Name, command.Nick.Nickname, command.Arguments["Message"], command.TimeStamp});
            }
            else
            {
                string maxMessage = "You already have the maximum number of introductions for this channel.  Delete one before trying to add another.";
                switch (command.MessageType)
                {
                    case MessageType.Channel:
                        Bot.IRC.SendPrivateMessage(command.Location, maxMessage);
                        break;
                    case MessageType.Query:
                        Bot.IRC.SendPrivateMessage(command.Nick.Nickname, maxMessage);
                        break;
                    case MessageType.Notice:
                        Bot.IRC.SendNotice(command.Nick.Nickname, maxMessage);
                        break;
                }
            }
        }
    }
}
