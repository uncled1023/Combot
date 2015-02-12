using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Combot.Databases;
using Combot.IRCServices.Messaging;

namespace Combot.Modules.Plugins
{
    public class Sed : Module
    {
        public override void Initialize()
        {
            Bot.IRC.Message.ChannelMessageReceivedEvent += HandleChannelMessageEvent;
        }

        private void HandleChannelMessageEvent(object sender, ChannelMessage message)
        {
            if (!Bot.ServerConfig.ChannelBlacklist.Contains(message.Channel)
                && !Bot.ServerConfig.NickBlacklist.Contains(message.Sender.Nickname)
                && !ChannelBlacklist.Contains(message.Channel)
                && !NickBlacklist.Contains(message.Sender.Nickname))
            {
                Regex sedRegex = new Regex(@"^s\/(?<Match>[^\/\\]*(?:\\.[^\/\\]*)*)\/(?<Replace>[^\/\\]*(?:\\.[^\/\\]*)*)\/(?<Option>[g|I|0-9]*)?");
                if (sedRegex.IsMatch(message.Message))
                {
                    Match sedMatch = sedRegex.Match(message.Message);
                    string match = sedMatch.Groups["Match"].ToString().Replace(@"\/", @"/");
                    string replace = sedMatch.Groups["Replace"].ToString().Replace(@"\/", @"/");
                    string option = sedMatch.Groups["Option"].ToString();
                    string mysqlCase;
                    RegexOptions matchOptions;
                    int optionVal;
                    int replaceNum;
                    if (int.TryParse(option, out optionVal))
                    {
                        matchOptions = RegexOptions.None;
                        replaceNum = optionVal;
                        mysqlCase = "CAST(`channelmessages`.`message` AS BINARY)";
                    }
                    else if (option == "g")
                    {
                        matchOptions = RegexOptions.None;
                        replaceNum = 1;
                        mysqlCase = "CAST(`channelmessages`.`message` AS BINARY)";
                    }
                    else if (option == "I")
                    {
                        matchOptions = RegexOptions.IgnoreCase;
                        replaceNum = 1;
                        mysqlCase = "`channelmessages`.`message`";
                    }
                    else
                    {
                        matchOptions = RegexOptions.None;
                        replaceNum = 1;
                        mysqlCase = "CAST(`channelmessages`.`message` AS BINARY)";
                    }
                    string mysqlMatch = match.Replace(@"\s", "[:space:]").Replace(@"\", @"\\");
                    List<Dictionary<string, object>> resultList = GetMessageList(message.Channel, message.Sender.Nickname, mysqlMatch, mysqlCase);
                    if (resultList.Any())
                    {
                        IEnumerable<Dictionary<string, object>> validList = resultList.Where(item => item["message"].ToString() != message.Message);
                        if (validList.Any())
                        {
                            string oldMessage = validList.First()["message"].ToString();
                            Regex messageRegex = new Regex(match, matchOptions);
                            string newMessage = messageRegex.Replace(oldMessage, replace, replaceNum);
                            string replacedMessage = string.Format("\u0002{0}\u0002 meant to say: {1}", message.Sender.Nickname, newMessage);
                            SendResponse(MessageType.Channel, message.Channel, message.Sender.Nickname, replacedMessage);
                        }
                        else
                        {
                            string noMatch = string.Format("You do not have any previous messages that match \u0002{0}\u0002.", match);
                            SendResponse(MessageType.Channel, message.Channel, message.Sender.Nickname, noMatch);
                        }
                    }
                    else
                    {
                        string noMatch = string.Format("You do not have any previous messages that match \u0002{0}\u0002.", match);
                        SendResponse(MessageType.Channel, message.Channel, message.Sender.Nickname, noMatch);
                    }
                }
            }
        }

        private List<Dictionary<string, object>> GetMessageList(string channel, string nickname, string regex, string caseString)
        {
            Database database = new Database(Bot.ServerConfig.Database);
            string search = "SELECT `channelmessages`.`message`, `channelmessages`.`date_added` FROM `channelmessages` " +
                            "INNER JOIN `nicks` " +
                            "ON `channelmessages`.`nick_id` = `nicks`.`id` " +
                            "INNER JOIN `channels` " +
                            "ON `channelmessages`.`channel_id` = `channels`.`id` " +
                            "INNER JOIN `servers` " +
                            "ON `channelmessages`.`server_id` = `servers`.`id` " +
                            "WHERE `servers`.`name` = {0} AND `channels`.`name` = {1} AND `nicks`.`nickname` = {2} AND " + caseString + " REGEXP {3} " +
                            "ORDER BY date_added DESC";
            return database.Query(search, new object[] { Bot.ServerConfig.Name, channel, nickname, regex });
        }
    }
}
