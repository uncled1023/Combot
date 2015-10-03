using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Threading;
using Combot.Databases;
using Combot.IRCServices.Messaging;

namespace Combot.Modules.Plugins
{
    public class Logging : Module
    {
        private const string SERVERLOGNAME = "--server--";
        private const string LOGFILENAME = "chat";
        private const string LOGFILEEXT = ".log";

        private static ReaderWriterLockSlim logLock;

        public override void Initialize()
        {
            logLock = new ReaderWriterLockSlim();

            Bot.IRC.ConnectEvent += AddServer;
            Bot.IRC.Message.CTCPMessageReceivedEvent += LogCTCPMessage;
            Bot.IRC.Message.CTCPNoticeReceivedEvent += LogCTCPNotice;
            Bot.IRC.Message.ChannelMessageReceivedEvent += LogChannelMessage;
            Bot.IRC.Message.PrivateMessageReceivedEvent += LogPrivateMessage;
            Bot.IRC.Message.ChannelNoticeReceivedEvent += LogChannelNotice;
            Bot.IRC.Message.PrivateNoticeReceivedEvent += LogPrivateNotice;
            Bot.IRC.Message.JoinChannelEvent += LogChannelJoin;
            Bot.IRC.Message.InviteChannelEvent += LogChannelInvite;
            Bot.IRC.Message.PartChannelEvent += LogChannelPart;
            Bot.IRC.Message.KickEvent += LogChannelKick;
            Bot.IRC.Message.QuitEvent += LogQuit;
            Bot.IRC.Message.NickChangeEvent += LogNickChange;
        }

        private void LogChannelMessage(object sender, ChannelMessage message)
        {
            if (!ChannelBlacklist.Contains(message.Channel)
                && !NickBlacklist.Contains(message.Sender.Nickname))
            {
                AddChannel(message.Channel);
                AddNick(message.Sender);
                string query = "INSERT INTO `channelmessages` SET " +
                               "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                               "`channel_id` = (SELECT `channels`.`id` FROM `channels` INNER JOIN `servers` ON `servers`.`id` = `channels`.`server_id` WHERE `servers`.`name` = {1} && `channels`.`name` = {2}), " +
                               "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {3} && `nickname` = {4}), " +
                               "`message` = {5}, " +
                               "`date_added` = {6}";
                Bot.Database.Execute(query, new object[] { Bot.ServerConfig.Name, Bot.ServerConfig.Name, message.Channel, Bot.ServerConfig.Name, message.Sender.Nickname, message.Message, message.TimeStamp });
            }
            LogToFile(message.Channel, message.TimeStamp, string.Format("<{0}> {1}", message.Sender.Nickname, message.Message));
        }

        private void LogPrivateMessage(object sender, PrivateMessage message)
        {
            if (!NickBlacklist.Contains(message.Sender.Nickname))
            {
                AddNick(message.Sender);
                string query = "INSERT INTO `privatemessages` SET " +
                               "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                               "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {1} && `nickname` = {2}), " +
                               "`message` = {3}, " +
                               "`date_added` = {4}";
                Bot.Database.Execute(query, new object[] { Bot.ServerConfig.Name, Bot.ServerConfig.Name, message.Sender.Nickname, message.Message, message.TimeStamp });
            }
            LogToFile(message.Sender.Nickname, message.TimeStamp, message.Message);
        }

        private void LogChannelNotice(object sender, ChannelNotice notice)
        {
            LogToFile(notice.Channel, notice.TimeStamp, string.Format("<{0}> {1}", notice.Sender.Nickname, notice.Message));
        }

        private void LogPrivateNotice(object sender, PrivateNotice notice)
        {
            LogToFile(SERVERLOGNAME, notice.TimeStamp, string.Format("<{0}> {1}", notice.Sender.Nickname, notice.Message));
        }

        private void LogCTCPMessage(object sender, CTCPMessage message)
        {
            LogToFile(SERVERLOGNAME, message.TimeStamp, string.Format("<{0}> CTCP {1} {2}", message.Sender.Nickname, message.Command, message.Arguments));
        }

        private void LogCTCPNotice(object sender, CTCPMessage notice)
        {
            LogToFile(SERVERLOGNAME, notice.TimeStamp, string.Format("<{0}> CTCP {1} {2}", notice.Sender.Nickname, notice.Command, notice.Arguments));
        }

        private void LogChannelJoin(object sender, JoinChannelInfo info)
        {
            if (!ChannelBlacklist.Contains(info.Channel)
                && !NickBlacklist.Contains(info.Nick.Nickname))
            {
                AddChannel(info.Channel);
                AddNick(info.Nick);
                string query = "INSERT INTO `channeljoins` SET " +
                               "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                               "`channel_id` = (SELECT `channels`.`id` FROM `channels` INNER JOIN `servers` ON `servers`.`id` = `channels`.`server_id` WHERE `servers`.`name` = {1} && `channels`.`name` = {2}), " +
                               "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {3} && `nicks`.`nickname` = {4}), " +
                               "`date_added` = {5}";
                Bot.Database.Execute(query, new object[] { Bot.ServerConfig.Name, Bot.ServerConfig.Name, info.Channel, Bot.ServerConfig.Name, info.Nick.Nickname, info.TimeStamp });
            }
            LogToFile(info.Channel, info.TimeStamp, string.Format("{0} has joined the channel.", info.Nick.Nickname));
        }

        private void LogChannelInvite(object sender, InviteChannelInfo info)
        {
            if (!ChannelBlacklist.Contains(info.Channel)
                && !NickBlacklist.Contains(info.Requester.Nickname))
            {
                AddChannel(info.Channel);
                AddNick(info.Requester);
                AddNick(info.Recipient);
                string query = "INSERT INTO `channelinvites` SET " +
                               "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                               "`channel_id` = (SELECT `channels`.`id` FROM `channels` INNER JOIN `servers` ON `servers`.`id` = `channels`.`server_id` WHERE `servers`.`name` = {1} && `channels`.`name` = {2}), " +
                               "`requester_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {3} && `nicks`.`nickname` = {4}), " +
                               "`recipient_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {5} && `nicks`.`nickname` = {6}), " +
                               "`date_invited` = {7}";
                Bot.Database.Execute(query, new object[] { Bot.ServerConfig.Name, Bot.ServerConfig.Name, info.Channel, Bot.ServerConfig.Name, info.Requester.Nickname, Bot.ServerConfig.Name, info.Recipient.Nickname, info.TimeStamp });
            }
            LogToFile(info.Channel, info.TimeStamp, string.Format("{0} has invited {1} to the channel.", info.Requester.Nickname, info.Recipient.Nickname));
        }

        private void LogChannelPart(object sender, PartChannelInfo info)
        {
            if (!ChannelBlacklist.Contains(info.Channel)
                && !NickBlacklist.Contains(info.Nick.Nickname))
            {
                AddChannel(info.Channel);
                AddNick(info.Nick);
                string query = "INSERT INTO `channelparts` SET " +
                               "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                               "`channel_id` = (SELECT `channels`.`id` FROM `channels` INNER JOIN `servers` ON `servers`.`id` = `channels`.`server_id` WHERE `servers`.`name` = {1} && `channels`.`name` = {2}), " +
                               "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {3} && `nickname` = {4}), " +
                               "`date_added` = {5}";
                Bot.Database.Execute(query, new object[] { Bot.ServerConfig.Name, Bot.ServerConfig.Name, info.Channel, Bot.ServerConfig.Name, info.Nick.Nickname, info.TimeStamp });
            }
            LogToFile(info.Channel, info.TimeStamp, string.Format("{0} has left the channel.", info.Nick.Nickname));
        }

        private void LogChannelKick(object sender, KickInfo info)
        {
            if (!ChannelBlacklist.Contains(info.Channel)
                && !NickBlacklist.Contains(info.KickedNick.Nickname))
            {
                AddChannel(info.Channel);
                AddNick(info.Nick);
                AddNick(info.KickedNick);
                string query = "INSERT INTO `channelkicks` SET " +
                               "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                               "`channel_id` = (SELECT `channels`.`id` FROM `channels` INNER JOIN `servers` ON `servers`.`id` = `channels`.`server_id` WHERE `servers`.`name` = {1} && `channels`.`name` = {2}), " +
                               "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {3} && `nickname` = {4}), " +
                               "`kicked_nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {5} && `nickname` = {6}), " +
                               "`reason` = {7}, " +
                               "`date_added` = {8}";
                Bot.Database.Execute(query, new object[] { Bot.ServerConfig.Name, Bot.ServerConfig.Name, info.Channel, Bot.ServerConfig.Name, info.Nick.Nickname, Bot.ServerConfig.Name, info.KickedNick.Nickname, info.Reason, info.TimeStamp });
            }
            LogToFile(info.Channel, info.TimeStamp, string.Format("{0} kicked {1} [{2}]", info.Nick.Nickname, info.KickedNick.Nickname, info.Reason));
        }

        private void LogQuit(object sender, QuitInfo info)
        {
            if (!NickBlacklist.Contains(info.Nick.Nickname))
            {
                AddNick(info.Nick);
                string query = "INSERT INTO `quits` SET " +
                               "`server_id` = (SELECT `id` FROM `servers` WHERE `name` = {0}), " +
                               "`nick_id` = (SELECT `nicks`.`id` FROM `nicks` INNER JOIN `servers` ON `servers`.`id` = `nicks`.`server_id` WHERE `servers`.`name` = {1} && `nickname` = {2}), " +
                               "`message` = {3}, " +
                               "`date_added` = {4}";
                Bot.Database.Execute(query, new object[] {Bot.ServerConfig.Name, Bot.ServerConfig.Name, info.Nick.Nickname, info.Message, info.TimeStamp});
            }
            LogToFile(SERVERLOGNAME, info.TimeStamp, string.Format("{0} has Quit.", info.Nick.Nickname));
        }

        private void LogNickChange(object sender, NickChangeInfo info)
        {
            if (!NickBlacklist.Contains(info.OldNick.Nickname) && !NickBlacklist.Contains(info.NewNick.Nickname))
            {
                AddNick(info.NewNick);
            }
            LogToFile(SERVERLOGNAME, info.TimeStamp, string.Format("{0} is now known as {1}", info.OldNick.Nickname, info.NewNick.Nickname));
        }

        private void LogToFile(string location, DateTime date, string log)
        {
            bool doLog = false;
            Boolean.TryParse(GetOptionValue("Log To File").ToString(), out doLog);
            if (doLog)
            {
                logLock.EnterWriteLock();
                string pattern = "[^a-zA-Z0-9-_.+#]"; //regex pattern
                string parsedLocation = Regex.Replace(location, pattern, "_");
                string logDir = Path.Combine(GetOptionValue("Log Path").ToString(), Bot.ServerConfig.Name, parsedLocation);
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                string logFile = Path.Combine(logDir, LOGFILENAME + LOGFILEEXT);
                // Check to see if we need to create a new log
                if (File.Exists(logFile))
                {
                    TrimLogFile(logDir);
                }
                // Write the log to the main log file
                StreamWriter logWriter = File.AppendText(logFile);
                logWriter.WriteLine(string.Format("[{0}] {1}", date.ToString("G"), log));
                logWriter.Close();
                logLock.ExitWriteLock();
            }
        }

        private void TrimLogFile(string logDir)
        {
            string logFile = Path.Combine(logDir, LOGFILENAME + LOGFILEEXT);
            int maxSize = 0;
            Int32.TryParse(GetOptionValue("Max Log Size").ToString(), out maxSize);
            FileInfo file = new FileInfo(logFile);
            long fileSize = file.Length;
            if (fileSize > maxSize)
            {
                // The file is too large, we need to increment the file names of the log files
                string[] files = Directory.GetFiles(logDir);
                for (int i = files.GetUpperBound(0) - 1; i >= 0; i--)
                {
                    string newFileName = LOGFILENAME + "_" + (i + 1) + LOGFILEEXT;
                    string newFile = Path.Combine(logDir, newFileName);
                    File.Move(files[i], newFile);
                }
            }
        }
    }
}
