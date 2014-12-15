using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Combot
{
    internal partial class IRCService
    {

        // ----------------------- //
        // Public Mapped Functions //
        // ----------------------- //

        /// <summary>
        /// Send a message to a nick
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendMessage(Nick nick, string message)
        {
            bool success = false;
            if (CanSendMessage())
            {
                IRCSendPrivMessage(nick.Nickname, message);
                success = true;
            }
            return success;
        }

        /// <summary>
        /// Send a message to a channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendMessage(Channel channel, string message)
        {
            bool success = false;
            if (CanSendMessage())
            {
                IRCSendPrivMessage(channel.Name, message);
                success = true;
            }
            return success;
        }

        /// <summary>
        /// Send a message to multiple nicks
        /// </summary>
        /// <param name="nicks"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendMessage(List<Nick> nicks, string message)
        {
            bool success = false;
            if (CanSendMessage())
            {
                List<string> nicknames = new List<string>();
                foreach (Nick nick in nicks)
                {
                    nicknames.Add(nick.Nickname);
                }
                IRCSendPrivMessage(nicknames, message);
                success = true;
            }
            return success;
        }

        /// <summary>
        /// Send a message to multiple channels
        /// </summary>
        /// <param name="channels"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendMessage(List<Channel> channels, string message)
        {
            bool success = false;
            if (CanSendMessage())
            {
                List<string> channelnames = new List<string>();
                foreach (Channel channel in channels)
                {
                    channelnames.Add(channel.Name);
                }
                IRCSendPrivMessage(channelnames, message);
                success = true;
            }
            return success;
        }

        public bool CanSendMessage() { return true; }

        /// <summary>
        /// Send a notice to a nick
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendNotice(Nick nick, string message)
        {
            bool success = false;
            if (CanSendNotice())
            {
                IRCSendNotice(nick.Nickname, message);
                success = true;
            }
            return success;
        }

        /// <summary>
        /// Send a notice to a channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendNotice(Channel channel, string message)
        {
            bool success = false;
            if (CanSendNotice())
            {
                IRCSendNotice(channel.Name, message);
                success = true;
            }
            return success;
        }

        /// <summary>
        /// Send a notice to multiple nicks
        /// </summary>
        /// <param name="nicks"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendNotice(List<Nick> nicks, string message)
        {
            bool success = false;
            if (CanSendNotice())
            {
                List<string> nicknames = new List<string>();
                foreach (Nick nick in nicks)
                {
                    nicknames.Add(nick.Nickname);
                }
                IRCSendNotice(nicknames, message);
                success = true;
            }
            return success;
        }

        /// <summary>
        /// Send a notice to multiple channels
        /// </summary>
        /// <param name="channels"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendNotice(List<Channel> channels, string message)
        {
            bool success = false;
            if (CanSendNotice())
            {
                List<string> channelnames = new List<string>();
                foreach (Channel channel in channels)
                {
                    channelnames.Add(channel.Name);
                }
                IRCSendNotice(channelnames, message);
                success = true;
            }
            return success;
        }

        public bool CanSendNotice() { return true; }

        /// <summary>
        /// Set your connection password
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool SendPassword(string password)
        {
            bool success = false;
            if (CanSendPassword())
            {
                IRCSendPassword(password);
                success = true;
            }
            return success;
        }

        public bool CanSendPassword() { return true; }

        /// <summary>
        /// Set your nickname
        /// </summary>
        /// <param name="nick"></param>
        /// <returns></returns>
        public bool SetNick(Nick nick)
        {
            bool success = false;
            if (CanSetNick())
            {
                IRCSendNick(nick.Nickname);
                success = true;
            }
            return success;
        }

        public bool CanSetNick() { return true; }

        /// <summary>
        /// Sets the user for the client connection
        /// </summary>
        /// <param name="nick"></param>
        /// <returns></returns>
        public bool SetUser(Nick nick)
        {
            bool success = false;
            if (CanSetUser())
            {
                IRCSendUser(nick.Nickname, nick.Host, _Bot.Config.Name, nick.Realname);
                success = true;
            }
            return success;
        }

        public bool CanSetUser() { return true; }


        public bool Oper(string username, string password)
        {
            bool success = false;
            if (CanOper())
            {
                IRCSendOper(username, password);
                success = true;
            }
            return success;
        }

        public bool CanOper() { return true; }

        public bool Quit()
        {
            bool success = false;
            if (CanQuit())
            {
                IRCSendQuit();
                success = true;
            }
            return success;
        }

        public bool Quit(string message)
        {
            bool success = false;
            if (CanQuit())
            {
                IRCSendQuit(message);
                success = true;
            }
            return success;
        }

        public bool CanQuit() { return true; }

        public bool Join(Channel channel)
        {
            bool success = false;
            if (CanJoin())
            {
                IRCSendJoin(channel.Name, channel.Key);
                success = true;
            }
            return success;
        }

        public bool IRCSendJoin(List<Channel> channels)
        {
            bool success = false;
            if (CanJoin())
            {
                List<string> channel_names = new List<string>();
                List<string> keys = new List<string>();
                foreach (Channel channel in channels)
                {
                    channel_names.Add(channel.Name);
                    keys.Add(channel.Key);
                }
                IRCSendJoin(channel_names, keys);
                success = true;
            }
            return success;
        }

        public bool CanJoin() { return true; }

        public bool IRCSendPart(string channel)
        {
            SendTCPMessage(string.Format("PART {0}", channel));
        }

        public bool IRCSendPart(List<string> channels)
        {
            string channel_list = string.Empty;
            foreach (string channel in channels)
            {
                channel_list += channel + ",";
            }

            SendTCPMessage(string.Format("PART {0}", channel_list.TrimEnd(',')));
        }

        public bool IRCSendMode(string channel, ChannelModeInfo modeInfo)
        {
            string mode_set = modeInfo.Set ? "+" : "-";
            SendTCPMessage(string.Format("MODE {0} {1} {2}", channel, mode_set + modeInfo.Mode.ToString(), modeInfo.Parameter));
        }

        public bool IRCSendMode(string channel, List<ChannelModeInfo> modeInfos)
        {
            foreach (ChannelModeInfo modeInfo in modeInfos)
            {
                IRCSendMode(channel, modeInfo);
            }
        }
        public bool IRCSendMode(string nick, UserModeInfo modeInfo)
        {
            string mode_set = modeInfo.Set ? "+" : "-";
            SendTCPMessage(string.Format("MODE {0} {1} {2}", nick, mode_set + modeInfo.Mode.ToString(), modeInfo.Parameter));
        }

        public bool IRCSendMode(string nick, List<UserModeInfo> modeInfos)
        {
            foreach (UserModeInfo modeInfo in modeInfos)
            {
                IRCSendMode(nick, modeInfo);
            }
        }

        public bool IRCSendTopic(string channel)
        {
            SendTCPMessage(string.Format("TOPIC {0}", channel));
        }

        public bool IRCSendTopic(string channel, string topic)
        {
            SendTCPMessage(string.Format("TOPIC {0} :{1}", channel, topic));
        }

        public bool IRCSendNames()
        {
            SendTCPMessage("NAMES");
        }

        public bool IRCSendNames(string channel)
        {
            SendTCPMessage(string.Format("NAMES {0}", channel));
        }

        public bool IRCSendNames(List<string> channels)
        {
            string channel_list = string.Empty;
            foreach (string channel in channels)
            {
                channel_list += channel + ",";
            }
            SendTCPMessage(string.Format("NAMES {0}", channel_list.TrimEnd(',')));
        }

        public bool IRCSendList()
        {
            SendTCPMessage("LIST");
        }

        public bool IRCSendList(string channel)
        {
            SendTCPMessage(string.Format("LIST {0}", channel));
        }

        public bool IRCSendList(List<string> channels)
        {
            string channel_list = string.Empty;
            foreach (string channel in channels)
            {
                channel_list += channel + ",";
            }
            SendTCPMessage(string.Format("LIST {0}", channel_list.TrimEnd(',')));
        }

        public bool IRCSendInvite(string channel, string nick)
        {
            SendTCPMessage(string.Format("INVITE {0} {1}", nick, channel));
        }

        public bool IRCSendKick(string channel, string nick)
        {
            SendTCPMessage(string.Format("KICK {0} {1}", channel, nick));
        }

        public bool IRCSendKick(string channel, string nick, string reason)
        {
            SendTCPMessage(string.Format("KICK {0} {1} :{2}", channel, nick, reason));
        }

        public bool IRCSendVersion(string server)
        {
            SendTCPMessage(string.Format("VERSION {0}", server));
        }

        public bool IRCSendStats(ServerStat stat)
        {
            SendTCPMessage(string.Format("STATS {0}", stat.ToString()));
        }

        public bool IRCSendStats(ServerStat stat, string parameter)
        {
            SendTCPMessage(string.Format("STATS {0} {1}", stat.ToString(), parameter));
        }

        public bool IRCSendLinks(string mask)
        {
            SendTCPMessage(string.Format("LINKS {0}", mask));
        }

        public bool IRCSendLinks(string server, string mask)
        {
            SendTCPMessage(string.Format("LINKS {0} {1}", mask, server));
        }

        public bool IRCSendTime()
        {
            SendTCPMessage("TIME");
        }

        public bool IRCSendTime(string server)
        {
            SendTCPMessage(string.Format("TIME {0}", server));
        }

        public bool IRCSendConnect(string server)
        {
            SendTCPMessage(string.Format("CONNECT {0}", server));
        }

        public bool IRCSendConnect(string server, string originator, int port)
        {
            SendTCPMessage(string.Format("CONNECT {0} {1} {2}", originator, port, server));
        }

        public bool IRCSendTrace(string target)
        {
            SendTCPMessage(string.Format("TRACE {0}", target));
        }

        public bool IRCSendAdmin()
        {
            SendTCPMessage("ADMIN");
        }

        public bool IRCSendAdmin(string host)
        {
            SendTCPMessage(string.Format("ADMIN {0}", host));
        }

        public bool IRCSendInfo(string host)
        {
            SendTCPMessage(string.Format("INFO {0}", host));
        }

        public bool IRCSendWho()
        {
            SendTCPMessage("WHO");
        }

        public bool IRCSendWho(string host, bool ops = false)
        {
            string msg = string.Empty;
            if (ops)
            {
                msg = string.Format("WHO {0} o", host);
            }
            else
            {
                msg = string.Format("WHO {0}", host);
            }
            SendTCPMessage(msg);
        }

        public bool IRCSendWhois(string nick)
        {
            SendTCPMessage(string.Format("WHOIS {0}", nick));
        }

        public bool IRCSendWhois(string nick, string server)
        {
            SendTCPMessage(string.Format("WHOIS {0} {1}", server, nick));
        }

        public bool IRCSendWhowas(string nick)
        {
            SendTCPMessage(string.Format("WHOIS {0}", nick));
        }

        public bool IRCSendWhowas(string nick, int entries)
        {
            SendTCPMessage(string.Format("WHOIS {0} {1}", nick, entries));
        }

        public bool IRCSendWhowas(string nick, int entries, string server)
        {
            SendTCPMessage(string.Format("WHOIS {0} {1} {2}", nick, entries, server));
        }

        public bool IRCSendKill(string nick, string comment)
        {
            SendTCPMessage(string.Format("KILL {0} {1}", nick, comment));
        }

        public bool IRCSendPing(string recipient)
        {
            SendTCPMessage(string.Format("PING {0}", recipient));
        }

        public bool IRCSendPong()
        {
            SendTCPMessage("PONG");
        }

        public bool IRCSendPong(string sender, string recipient)
        {
            SendTCPMessage(string.Format("PONG {0} {1}", sender, recipient));
        }

        public bool IRCSendAway()
        {
            SendTCPMessage("AWAY");
        }

        public bool IRCSendAway(string message)
        {
            SendTCPMessage(string.Format("AWAY {0}", message));
        }

        public bool IRCSendRehash()
        {
            SendTCPMessage("REHASH");
        }

        public bool IRCSendRestart()
        {
            SendTCPMessage("RESTART");
        }

        public bool IRCSendSummon()
        {
            SendTCPMessage("SUMMON");
        }

        public bool IRCSendSummon(string nick)
        {
            SendTCPMessage(string.Format("SUMMON {0}", nick));
        }

        public bool IRCSendSummon(string nick, string host)
        {
            SendTCPMessage(string.Format("SUMMON {0} {1}", nick, host));
        }

        public bool IRCSendUsers(string server)
        {
            SendTCPMessage(string.Format("USERS {0}", server));
        }

        public bool IRCSendWallops(string message)
        {
            SendTCPMessage(string.Format("WALLOPS :{0}", message));
        }

        public bool IRCSendUserhost(List<string> nicks)
        {
            string message = string.Empty;
            foreach (string nick in nicks)
            {
                message += " " + nick;
            }
            SendTCPMessage(string.Format("USERHOST {0}", message.Trim()));
        }

        public bool IRCSendIson(List<string> nicks)
        {
            string message = string.Empty;
            foreach (string nick in nicks)
            {
                message += " " + nick;
            }
            SendTCPMessage(string.Format("ISON {0}", message.Trim()));
        }

        // ------------------- //
        // Internal Functions  //
        // ------------------- //

        /// <summary>
        /// Sends a private message to a nick or channel
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="message"></param>
        protected void IRCSendPrivMessage(string recipient, string message)
        {
            SendTCPMessage(string.Format("PRIVMSG {0} :{1}", recipient, message));
        }

        protected void IRCSendPrivMessage(List<string> recipients, string message)
        {
            string recipient_list = string.Empty;
            foreach (string recipient in recipients)
            {
                recipient_list += recipient + ",";
            }

            SendTCPMessage(string.Format("PRIVMSG {0} :{1}", recipient_list.TrimEnd(','), message));
        }

        /// <summary>
        /// Sends a Notice to either a nick or channel
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="message"></param>
        protected void IRCSendNotice(string recipient, string message)
        {
            SendTCPMessage(string.Format("NOTICE {0} :{1}", recipient, message));
        }

        protected void IRCSendNotice(List<string> recipients, string message)
        {
            string recipient_list = string.Empty;
            foreach (string recipient in recipients)
            {
                recipient_list += recipient + ",";
            }

            SendTCPMessage(string.Format("NOTICE {0} :{1}", recipient_list.TrimEnd(','), message));
        }

        /// <summary>
        /// Sends the connection password
        /// </summary>
        /// <param name="password"></param>
        protected void IRCSendPassword(string password)
        {
            SendTCPMessage(password);
        }

        /// <summary>
        /// Sends a Nick command to set the nickname
        /// </summary>
        /// <param name="nick"></param>
        protected void IRCSendNick(string nick)
        {
            SendTCPMessage(nick);
        }

        /// <summary>
        /// Sends the User command to set a user
        /// </summary>
        /// <param name="user"></param>
        protected void IRCSendUser(string username, string hostname, string servername, string realname)
        {
            SendTCPMessage(string.Format("USER {0} {1} {2} :{3}", username, hostname, servername, realname));
        }

        /// <summary>
        /// Sends the Oper command to authorize the client as a newtork Oper
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        protected void IRCSendOper(string username, string password)
        {
            SendTCPMessage(string.Format("OPER {0} {1}", username, password));
        }

        /// <summary>
        /// Sends a Quit command to end the client session
        /// </summary>
        /// <param name="message"></param>
        protected void IRCSendQuit()
        {
            SendTCPMessage("QUIT");
        }

        protected void IRCSendQuit(string message)
        {
            SendTCPMessage(string.Format("QUIT :{0}", message));
        }

        /// <summary>
        /// Sends a Join command to join a channel
        /// </summary>
        /// <param name="channel"></param>
        protected void IRCSendJoin(string channel, string key = "")
        {
            string message = string.Empty;
            message = (key != string.Empty) ? string.Format("{0}; {1}", channel, key) : channel;
            SendTCPMessage(string.Format("JOIN {0}", message));
        }

        protected void IRCSendJoin(List<string> channels, List<string> keys)
        {
            string message = string.Empty;
            string channel_string = string.Empty;
            string key_string = string.Empty;

            foreach (string channel in channels)
            {                
                channel_string += channel + ",";
            }
            foreach (string key in keys)
            {
                if (key != string.Empty)
                {
                    key_string += key + ",";
                }
            }
            channel_string = channel_string.TrimEnd(',');
            key_string = key_string.TrimEnd(',');

            message = (key_string != string.Empty) ? string.Format("{0}; {1}", channel_string, key_string) : channel_string;
            SendTCPMessage(string.Format("JOIN {0}", message));
        }

        /// <summary>
        /// Sends a Part command to leave a channel
        /// </summary>
        /// <param name="channel"></param>
        protected void IRCSendPart(string channel)
        {
            SendTCPMessage(string.Format("PART {0}", channel));
        }

        protected void IRCSendPart(List<string> channels)
        {
            string channel_list = string.Empty;
            foreach (string channel in channels)
            {
                channel_list += channel + ",";
            }

            SendTCPMessage(string.Format("PART {0}", channel_list.TrimEnd(',')));
        }


        /// <summary>
        /// Sends a Mode command for either a channel mode or user mode
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="mode"></param>
        protected void IRCSendMode(string channel, ChannelModeInfo modeInfo)
        {
            string mode_set = modeInfo.Set ? "+" : "-";
            SendTCPMessage(string.Format("MODE {0} {1} {2}", channel, mode_set + modeInfo.Mode.ToString(), modeInfo.Parameter));
        }

        protected void IRCSendMode(string channel, List<ChannelModeInfo> modeInfos)
        {
            foreach (ChannelModeInfo modeInfo in modeInfos)
            {
                IRCSendMode(channel, modeInfo);
            }
        }
        protected void IRCSendMode(string nick, UserModeInfo modeInfo)
        {
            string mode_set = modeInfo.Set ? "+" : "-";
            SendTCPMessage(string.Format("MODE {0} {1} {2}", nick, mode_set + modeInfo.Mode.ToString(), modeInfo.Parameter));
        }

        protected void IRCSendMode(string nick, List<UserModeInfo> modeInfos)
        {
            foreach (UserModeInfo modeInfo in modeInfos)
            {
                IRCSendMode(nick, modeInfo);
            }
        }

        /// <summary>
        /// Sends a Topic command to change the channels topic or view the current one
        /// </summary>
        /// <param name="channel"></param>
        protected void IRCSendTopic(string channel)
        {
            SendTCPMessage(string.Format("TOPIC {0}", channel));
        }

        protected void IRCSendTopic(string channel, string topic)
        {
            SendTCPMessage(string.Format("TOPIC {0} :{1}", channel, topic));
        }

        /// <summary>
        /// Sends a Names command to get a list of visible users
        /// </summary>
        protected void IRCSendNames()
        {
            SendTCPMessage("NAMES");
        }

        protected void IRCSendNames(string channel)
        {
            SendTCPMessage(string.Format("NAMES {0}", channel));
        }

        protected void IRCSendNames(List<string> channels)
        {
            string channel_list = string.Empty;
            foreach (string channel in channels)
            {
                channel_list += channel + ",";
            }
            SendTCPMessage(string.Format("NAMES {0}", channel_list.TrimEnd(',')));
        }

        /// <summary>
        /// Sends a List command to get the topic of channels
        /// </summary>
        protected void IRCSendList()
        {
            SendTCPMessage("LIST");
        }

        protected void IRCSendList(string channel)
        {
            SendTCPMessage(string.Format("LIST {0}", channel));
        }

        protected void IRCSendList(List<string> channels)
        {
            string channel_list = string.Empty;
            foreach (string channel in channels)
            {
                channel_list += channel + ",";
            }
            SendTCPMessage(string.Format("LIST {0}", channel_list.TrimEnd(',')));
        }

        /// <summary>
        /// Sends an Invite command that invites the specified nick to the channel 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="nick"></param>
        protected void IRCSendInvite(string channel, string nick)
        {
            SendTCPMessage(string.Format("INVITE {0} {1}", nick, channel));
        }

        /// <summary>
        /// Sends a Kick command to remove a user from a channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="nick"></param>
        protected void IRCSendKick(string channel, string nick)
        {
            SendTCPMessage(string.Format("KICK {0} {1}", channel, nick));
        }

        protected void IRCSendKick(string channel, string nick, string reason)
        {
            SendTCPMessage(string.Format("KICK {0} {1} :{2}", channel, nick, reason));
        }

        /// <summary>
        /// Sends a Version command to the server to get a Version reply
        /// </summary>
        /// <param name="server"></param>
        protected void IRCSendVersion(string server)
        {
            SendTCPMessage(string.Format("VERSION {0}", server));
        }

        /// <summary>
        /// Sends a Stats command to view Server information and statistics
        /// </summary>
        /// <param name="stat"></param>
        protected void IRCSendStats(ServerStat stat)
        {
            SendTCPMessage(string.Format("STATS {0}", stat.ToString()));
        }

        protected void IRCSendStats(ServerStat stat, string parameter)
        {
            SendTCPMessage(string.Format("STATS {0} {1}", stat.ToString(), parameter));
        }

        /// <summary>
        /// Sends a Links command to list all servers matching a mask
        /// </summary>
        /// <param name="mask"></param>
        protected void IRCSendLinks(string mask)
        {
            SendTCPMessage(string.Format("LINKS {0}", mask));
        }

        protected void IRCSendLinks(string server, string mask)
        {
            SendTCPMessage(string.Format("LINKS {0} {1}", mask, server));
        }

        /// <summary>
        /// Sends a Time command to query the local server time
        /// </summary>
        protected void IRCSendTime()
        {
            SendTCPMessage("TIME");
        }

        protected void IRCSendTime(string server)
        {
            SendTCPMessage(string.Format("TIME {0}", server));
        }

        /// <summary>
        /// Senda a Connect command to have the server try to connect to another server
        /// </summary>
        /// <param name="server"></param>
        protected void IRCSendConnect(string server)
        {
            SendTCPMessage(string.Format("CONNECT {0}", server));
        }

        protected void IRCSendConnect(string server, string originator, int port)
        {
            SendTCPMessage(string.Format("CONNECT {0} {1} {2}", originator, port, server));
        }

        /// <summary>
        /// Sends a Trace command to find the route to the target (nick or server)
        /// </summary>
        /// <param name="target"></param>
        protected void IRCSendTrace(string target)
        {
            SendTCPMessage(string.Format("TRACE {0}", target));
        }

        /// <summary>
        /// Sends an Admin command to get the name of the server Administrator
        /// </summary>
        protected void IRCSendAdmin()
        {
            SendTCPMessage("ADMIN");
        }

        protected void IRCSendAdmin(string host)
        {
            SendTCPMessage(string.Format("ADMIN {0}", host));
        }

        /// <summary>
        /// Sends an Info command for a specific server or nick
        /// </summary>
        /// <param name="host"></param>
        protected void IRCSendInfo(string host)
        {
            SendTCPMessage(string.Format("INFO {0}", host));
        }

        /// <summary>
        /// Sends a Who command to list all public users or matching a mask
        /// </summary>
        protected void IRCSendWho()
        {
            SendTCPMessage("WHO");
        }

        protected void IRCSendWho(string host, bool ops = false)
        {
            string msg = string.Empty;
            if (ops)
            {
                msg = string.Format("WHO {0} o", host);
            }
            else
            {
                msg = string.Format("WHO {0}", host);
            }
            SendTCPMessage(msg);
        }

        /// <summary>
        /// Sends a Whois command to get info about a user
        /// </summary>
        /// <param name="nick"></param>
        protected void IRCSendWhois(string nick)
        {
            SendTCPMessage(string.Format("WHOIS {0}", nick));
        }

        protected void IRCSendWhois(string nick, string server)
        {
            SendTCPMessage(string.Format("WHOIS {0} {1}", server, nick));
        }

        /// <summary>
        /// Sends a Whowas command to get the nick history of a user
        /// </summary>
        /// <param name="nick"></param>
        protected void IRCSendWhowas(string nick)
        {
            SendTCPMessage(string.Format("WHOIS {0}", nick));
        }

        protected void IRCSendWhowas(string nick, int entries)
        {
            SendTCPMessage(string.Format("WHOIS {0} {1}", nick, entries));
        }

        protected void IRCSendWhowas(string nick, int entries, string server)
        {
            SendTCPMessage(string.Format("WHOIS {0} {1} {2}", nick, entries, server));
        }

        /// <summary>
        /// Sends a Kill command to disconnect a nick
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="comment"></param>
        protected void IRCSendKill(string nick, string comment)
        {
            SendTCPMessage(string.Format("KILL {0} {1}", nick, comment));
        }

        /// <summary>
        /// Sends a Ping command to the recipient
        /// </summary>
        /// <param name="recipient"></param>
        protected void IRCSendPing(string recipient)
        {
            SendTCPMessage(string.Format("PING {0}", recipient));
        }

        /// <summary>
        /// Sends a PONG response to respond to a Ping
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="recipient"></param>
        protected void IRCSendPong()
        {
            SendTCPMessage("PONG");
        }

        protected void IRCSendPong(string sender, string recipient)
        {
            SendTCPMessage(string.Format("PONG {0} {1}", sender, recipient));
        }


        /// <summary>
        /// Sends an Away command to unset away status
        /// </summary>
        protected void IRCSendAway()
        {
            SendTCPMessage("AWAY");
        }

        /// <summary>
        /// Sends an Away comand to set away status with auto-reply message
        /// </summary>
        /// <param name="message"></param>
        protected void IRCSendAway(string message)
        {
            SendTCPMessage(string.Format("AWAY {0}", message));
        }

        /// <summary>
        /// Sends a Rehash command to the server to reload it's configuration file
        /// </summary>
        protected void IRCSendRehash()
        {
            SendTCPMessage("REHASH");
        }

        /// <summary>
        /// Sends a Restart command to the server to restart
        /// </summary>
        protected void IRCSendRestart()
        {
            SendTCPMessage("RESTART");
        }

        /// <summary>
        /// Sends a Summon command to summon a nick to the server
        /// </summary>
        /// <param name="nick"></param>
        protected void IRCSendSummon()
        {
            SendTCPMessage("SUMMON");
        }

        protected void IRCSendSummon(string nick)
        {
            SendTCPMessage(string.Format("SUMMON {0}", nick));
        }

        protected void IRCSendSummon(string nick, string host)
        {
            SendTCPMessage(string.Format("SUMMON {0} {1}", nick, host));
        }

        /// <summary>
        /// Sends a Users command to get a list of Users from a server
        /// </summary>
        /// <param name="server"></param>
        protected void IRCSendUsers(string server)
        {
            SendTCPMessage(string.Format("USERS {0}", server));
        }

        /// <summary>
        /// Sends a Wallops command which sends a message to all connected ops
        /// </summary>
        /// <param name="message"></param>
        protected void IRCSendWallops(string message)
        {
            SendTCPMessage(string.Format("WALLOPS :{0}", message));
        }

        /// <summary>
        /// Sends an Userhost command to up to 5 nicknames to return information about each nick
        /// </summary>
        /// <param name="nicks"></param>
        protected void IRCSendUserhost(List<string> nicks)
        {
            string message = string.Empty;
            foreach (string nick in nicks)
            {
                message += " " + nick;
            }
            SendTCPMessage(string.Format("USERHOST {0}", message.Trim()));
        }

        /// <summary>
        /// Sends an IsOn command to get a return if the nicks specified are online
        /// </summary>
        /// <param name="nicks"></param>
        protected void IRCSendIson(List<string> nicks)
        {
            string message = string.Empty;
            foreach (string nick in nicks)
            {
                message += " " + nick;
            }
            SendTCPMessage(string.Format("ISON {0}", message.Trim()));
        }
    }
}
