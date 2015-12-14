using System;
using System.Collections.Generic;
using System.Linq;
using Combot.Databases;

namespace Combot.Modules.Plugins
{
    public class Quotes : Module
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
                case "Quote":
                    GetQuote(command);
                    break;
            }
        }

        private void GetQuote(CommandMessage command)
        {
            string channel = command.Arguments.ContainsKey("Channel") ? command.Arguments["Channel"] : command.Location;
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            if (command.Arguments.ContainsKey("Nickname"))
            {
                results = GetQuoteList(channel, command.Arguments["Nickname"]);
            }
            else
            {
                results = GetQuoteList(channel);
            }
            if (results.Any())
            {
                Random randNum = new Random();
                int index = randNum.Next(results.Count - 1);
                Dictionary<string, object> quote = GetQuote(Convert.ToInt32(results[index]["id"])).First();
                string quoteMessage = string.Format("[{0}] {1}", quote["nickname"], quote["message"]);
                SendResponse(command.MessageType, command.Location, command.Nick.Nickname, quoteMessage);
            }
            else
            {
                if (command.Arguments.ContainsKey("Nickname"))
                {
                    string quoteMessage = string.Format("There are no quotes for \u0002{0}\u0002", command.Arguments["Nickname"]);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, quoteMessage);
                }
                else
                {
                    string quoteMessage = string.Format("There are no quotes for \u0002{0}\u0002.", channel);
                    SendResponse(command.MessageType, command.Location, command.Nick.Nickname, quoteMessage);
                }
            }
        }

        private List<Dictionary<string, object>> GetQuoteList(string channel)
        {
            string search = @"SELECT `channelmessages`.`id` FROM `channelmessages` 
                                INNER JOIN `channels` 
                                ON `channelmessages`.`channel_id` = `channels`.`id` 
                                INNER JOIN `servers` 
                                ON `channelmessages`.`server_id` = `servers`.`id` 
                                WHERE `servers`.`name` = {0} AND `channels`.`name` = {1}";
            return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, channel });
        }

        private List<Dictionary<string, object>> GetQuoteList(string channel, string nickname)
        {
            string search = @"SELECT `channelmessages`.`id` FROM `channelmessages` 
                                INNER JOIN `nicks` 
                                ON `channelmessages`.`nick_id` = `nicks`.`id` 
                                INNER JOIN `channels` 
                                ON `channelmessages`.`channel_id` = `channels`.`id` 
                                INNER JOIN `servers` 
                                ON `channelmessages`.`server_id` = `servers`.`id` 
                                WHERE `servers`.`name` = {0} AND `channels`.`name` = {1} AND `nicks`.`nickname` = {2}";
            return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, channel, nickname });
        }

        private List<Dictionary<string, object>> GetQuote(int id)
        {
            string search = @"SELECT `channelmessages`.`message`, `nicks`.`nickname` FROM `channelmessages` 
                                INNER JOIN `nicks` 
                                ON `channelmessages`.`nick_id` = `nicks`.`id` 
                                WHERE `channelmessages`.`id` = {0}";
            return Bot.Database.Query(search, new object[] { id });
        }

        private List<Dictionary<string, object>> GetQuote(string channel)
        {
            string search = @"
                                set max_heap_table_size = 33554432;

                                CREATE TEMPORARY TABLE IF NOT EXISTS tempQuotes
                                (tempID BIGINT NOT NULL AUTO_INCREMENT,  
                                 id INT(11) NOT NULL,
                                PRIMARY KEY tempID (tempID), UNIQUE KEY id (id))
                                ENGINE=memory 
                                AS (
                                    SELECT `channelmessages`.`id` 
                                    FROM `channelmessages` 
                                    INNER JOIN `channels` 
                                        ON `channelmessages`.`channel_id` = `channels`.`id` 
                                    INNER JOIN `servers` 
                                        ON `channelmessages`.`server_id` = `servers`.`id` 
                                    WHERE `channels`.`name` = {1} AND `servers`.`name` = {0}
                                );

                                CREATE TEMPORARY TABLE tempQuotes2 LIKE tempQuotes;
                                INSERT INTO tempQuotes2 SELECT * FROM tempQuotes;

                                SELECT r1.id
                                  FROM tempQuotes AS r1 JOIN
                                       (SELECT CEIL(RAND() *
                                                     (SELECT MAX(tempID)
                                                        FROM tempQuotes2)) AS tempID)
                                        AS r2
                                 WHERE r1.tempID >= r2.tempID
                                 ORDER BY r1.tempID ASC
                                 LIMIT 1;";
            return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, channel });
        }

        private List<Dictionary<string, object>> GetQuote(string channel, string nickname)
        {
            string search = @"
                                set max_heap_table_size = 33554432;

                                CREATE TEMPORARY TABLE IF NOT EXISTS tempQuotes
                                (tempID BIGINT NOT NULL AUTO_INCREMENT, 
                                 id INT(11) NOT NULL,
                                PRIMARY KEY tempID (tempID), UNIQUE KEY id (id))
                                ENGINE=memory 
                                AS (
                                    SELECT `channelmessages`.`id` 
                                    FROM `channelmessages` 
                                    INNER JOIN `nicks` 
                                        ON `channelmessages`.`nick_id` = `nicks`.`id`                                         
                                    INNER JOIN `channels` 
                                        ON `channelmessages`.`channel_id` = `channels`.`id` 
                                    INNER JOIN `servers` 
                                        ON `channelmessages`.`server_id` = `servers`.`id` 
                                    WHERE `nicks`.`nickname` = {2} AND `channels`.`name` = {1} AND `servers`.`name` = {0}
                                );

                                CREATE TEMPORARY TABLE tempQuotes2 LIKE tempQuotes;
                                INSERT INTO tempQuotes2 SELECT * FROM tempQuotes;

                                SELECT r1.id
                                  FROM tempQuotes AS r1 JOIN
                                       (SELECT CEIL(RAND() *
                                                     (SELECT MAX(tempID)
                                                        FROM tempQuotes2)) AS tempID)
                                        AS r2
                                 WHERE r1.tempID >= r2.tempID
                                 ORDER BY r1.tempID ASC
                                 LIMIT 1;";
            return Bot.Database.Query(search, new object[] { Bot.ServerConfig.Name, channel, nickname });
        }
    }
}
