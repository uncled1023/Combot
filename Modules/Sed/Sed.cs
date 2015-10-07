using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Combot.Databases;
using Combot.IRCServices.Messaging;

namespace Combot.Modules.Plugins
{
    public class Sed : Module
    {
        private Dictionary<string, List<string>> LastMessages;
        private readonly ReaderWriterLockSlim MessageLock = new ReaderWriterLockSlim();

        public override void Initialize()
        {
            LastMessages = new Dictionary<string, List<string>>();
            Bot.IRC.Message.ChannelMessageReceivedEvent += HandleChannelMessageEvent;
        }

        private void HandleChannelMessageEvent(object sender, ChannelMessage message)
        {
            if (Enabled
                && !Bot.ServerConfig.ChannelBlacklist.Contains(message.Channel)
                && !Bot.ServerConfig.NickBlacklist.Contains(message.Sender.Nickname)
                && !ChannelBlacklist.Contains(message.Channel)
                && !NickBlacklist.Contains(message.Sender.Nickname))
            {
                bool nickEnabled = false;
                Boolean.TryParse(GetOptionValue("Nickname Enabled").ToString(), out nickEnabled);
                bool channelEnabled = false;
                Boolean.TryParse(GetOptionValue("Channel Enabled").ToString(), out channelEnabled);
                int maxMessages = 10;
                Int32.TryParse(GetOptionValue("Max Messages").ToString(), out maxMessages);

                string key = string.Empty;
                if (nickEnabled && channelEnabled)
                {
                    key = string.Format("{0} {1}", message.Channel, message.Sender.Nickname);
                }
                else if (nickEnabled)
                {
                    key = message.Sender.Nickname;
                }
                else if (channelEnabled)
                {
                    key = message.Channel;
                }
                else
                {
                    key = string.Empty;
                }

                Regex sedRegex = new Regex(@"^s\/(?<Match>[^\/\\]*(?:\\.[^\/\\]*)*)\/(?<Replace>[^\/\\]*(?:\\.[^\/\\]*)*)\/(?<Option>[g|I|0-9]*)?");
                if (sedRegex.IsMatch(message.Message))
                {
                    Match sedMatch = sedRegex.Match(message.Message);
                    string match = sedMatch.Groups["Match"].ToString().Replace(@"\/", @"/");
                    string replace = sedMatch.Groups["Replace"].ToString().Replace(@"\/", @"/");
                    string option = sedMatch.Groups["Option"].ToString();
                    RegexOptions matchOptions;
                    int optionVal;
                    int replaceNum;
                    if (int.TryParse(option, out optionVal))
                    {
                        matchOptions = RegexOptions.None;
                        replaceNum = optionVal;
                    }
                    else if (option == "g")
                    {
                        matchOptions = RegexOptions.None;
                        replaceNum = 1;
                    }
                    else if (option == "I")
                    {
                        matchOptions = RegexOptions.IgnoreCase;
                        replaceNum = 1;
                    }
                    else
                    {
                        matchOptions = RegexOptions.None;
                        replaceNum = 1;
                    }
                    bool foundResult = false;
                    MessageLock.EnterWriteLock();
                    if (LastMessages.ContainsKey(key))
                    {
                        List<string> msgList = new List<string>();
                        msgList.AddRange(LastMessages[key]);
                        msgList.Reverse();
                        foreach (string msg in msgList)
                        {
                            Regex messageRegex = new Regex(match, matchOptions);
                            if (messageRegex.IsMatch(msg))
                            {
                                string newMessage = messageRegex.Replace(msg, replace, replaceNum);
                                string replacedMessage = string.Format("\u0002{0}\u0002 meant to say: {1}", message.Sender.Nickname, newMessage);
                                SendResponse(MessageType.Channel, message.Channel, message.Sender.Nickname, replacedMessage);
                                foundResult = true;
                                break;
                            }
                        }
                    }
                    MessageLock.ExitWriteLock();
                    if (!foundResult)
                    {
                        string noMatch = string.Format("You do not have any previous messages that match \u0002{0}\u0002.", match);
                        SendResponse(MessageType.Channel, message.Channel, message.Sender.Nickname, noMatch, true);
                    }
                }
                else
                {
                    // Add or replace the message for the user/channel
                    MessageLock.EnterWriteLock();
                    if (LastMessages.ContainsKey(key))
                    {
                        List<string> messages = LastMessages[key];
                        if (messages.Count >= maxMessages)
                        {
                            messages.RemoveAt(0);
                        }
                        messages.Add(message.Message);
                        LastMessages[key] = messages;
                    }
                    else
                    {
                        LastMessages.Add(key, new List<string> { message.Message });
                    }
                    MessageLock.ExitWriteLock();
                }
            }
        }
    }
}
