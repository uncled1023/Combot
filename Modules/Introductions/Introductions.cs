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
                            EditIntroduction(command);
                            break;
                        case "del":
                            DeleteIntroduction(command);
                            break;
                        case "view":
                            ViewIntroduction(command);
                            break;
                    }
                    break;
            }
        }

        private void HandleJoinEvent(object sender, JoinChannelInfo info)
        {
            if (!Bot.ServerConfig.ChannelBlacklist.Contains(info.Channel)
                    && !Bot.ServerConfig.NickBlacklist.Contains(info.Nick.Nickname)
                    && !ChannelBlacklist.Contains(info.Channel)
                    && !NickBlacklist.Contains(info.Nick.Nickname))
            {
                List<Dictionary<string, object>> results = GetIntroductionList(info.Channel, info.Nick.Nickname);
                if (results.Any())
                {
                    Random randNum = new Random();
                    int index = randNum.Next(0, results.Count);
                    Dictionary<string, object> intro = results[index];
                    Bot.IRC.SendPrivateMessage(info.Channel, string.Format("\u200B{0}", intro["message"]));
                }
            }
        }

        private void AddIntroduction(CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            List<Dictionary<string, object>> results = GetIntroductionList(channel, command.Nick.Nickname);

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
                Bot.Database.Execute(query, new object[] { Bot.ServerConfig.Name, Bot.ServerConfig.Name, channel, Bot.ServerConfig.Name, command.Nick.Nickname, command.Arguments["Message"], command.TimeStamp });
                string introMessage = string.Format("Added introduction.  You now have \u0002{0}\u0002 introductions set.", results.Count + 1);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, introMessage);
            }
            else
            {
                string maxMessage = "You already have the maximum number of introductions for this channel.  Delete one before trying to add another.";
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, maxMessage);
            }
        }

        /* Returns the parsed ID field if valid, otherwise returns 0 */
        private int HasValidIntroductionID(CommandMessage command){
            int num = 0;
            int ret = 0;
            List<Dictionary<string, object>> results = GetIntroductionList(channel, command.Nick.Nickname);

            if (int.TryParse(command.Arguments["ID"], out num))
            {
                if (results.Count >= num && num > 0)
                {
                    ret = num;
                }
            }

            return ret;
        }

        private void EditIntroduction(CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            List<Dictionary<string, object>> results = GetIntroductionList(channel, command.Nick.Nickname);
            int num = HasValidIntroductionID( command );

            if (num > 0){
                int id = Convert.ToInt32(results[num - 1]["id"]);
                string query = "UPDATE `introductions` SET " +
                    "`message` = {0} " +
                    "WHERE `id` = {1}";
                Bot.Database.Execute(query, new object[] { command.Arguments["Message"], id });
                string introMessage = string.Format("Introduction #\u0002{0}\u0002 is now: {1}", num, command.Arguments["Message"]);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, introMessage);
            }
            else
            {
                string invalid = "Invalid introduction ID.";
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid);
            }
        }

        private void DeleteIntroduction(CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            List<Dictionary<string, object>> results = GetIntroductionList(channel, command.Nick.Nickname);
            int num = HasValidIntroductionID( command );

            if (num > 0){
                int id = Convert.ToInt32(results[num - 1]["id"]);
                string query = "DELETE FROM `introductions` " +
                    "WHERE `id` = {0}";
                Bot.Database.Execute(query, new object[] { id });
                string introMessage = string.Format("Introduction #\u0002{0}\u0002 has been deleted.", num);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, introMessage);
            }
            else
            {
                string invalid = "Invalid introduction ID.";
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid);
            }
        }

        private void ViewIntroduction(CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            List<Dictionary<string, object>> results = GetIntroductionList(channel, command.Nick.Nickname);
            int num = 0;

            if (command.Arguments.ContainsKey("ID"))
            {
                num = HasValidIntroductionID( command );
                if ( num > 0 ){
                    string introMessage = string.Format("Introduction #\u0002{0}\u0002: {1}", num, results[num - 1]["message"]);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, introMessage);
                }
                else
                {
                    string invalid = "Invalid introduction ID.";
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid);
                }
            }
            else
            {
                if (results.Any())
                {
                    for (int i = 0; i < results.Count; i++)
                    {
                        string introMessage = string.Format("Introduction #\u0002{0}\u0002: {1}", i + 1, results[i]["message"]);
                        SendResponse(command.MessageType, command.Location, command.Nick.Nickname, introMessage);
                    }
                }
                else
                {
                    string invalid = "You do not have any introductions set.";
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, invalid);
                }
            }
        }

        private List<Dictionary<string, object>> GetIntroductionList(string channel, string nickname)
        {
            // Check to see if they have reached the max number of introductions
            string search = "SELECT `introductions`.`id`, `introductions`.`message` FROM `introductions` " +
                "INNER JOIN `nicks` " +
                "ON `introductions`.`nick_id` = `nicks`.`id` " +
                "INNER JOIN `channels` " +
                "ON `introductions`.`channel_id` = `channels`.`id` " +
                "INNER JOIN `servers` " +
                "ON `nicks`.`server_id` = `servers`.`id` " +
                "WHERE `servers`.`name` = {0} AND `channels`.`name` = {1} AND `nicks`.`nickname` = {2}";
            return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, channel, nickname });
        }
    }
}
