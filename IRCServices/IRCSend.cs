using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Combot.IRCServices
{
    public partial class IRC
    {
        /// <summary>
        /// Sends a private message to a nick or channel
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="message"></param>
        public void SendPrivateMessage(string recipient, string message)
        {
            SendTCPMessage(string.Format("PRIVMSG {0} :{1}", recipient, message));
        }

        public void SendPrivateMessage(List<string> recipients, string message)
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
        public void SendNotice(string recipient, string message)
        {
            SendTCPMessage(string.Format("NOTICE {0} :{1}", recipient, message));
        }

        public void SendNotice(List<string> recipients, string message)
        {
            string recipient_list = string.Empty;
            foreach (string recipient in recipients)
            {
                recipient_list += recipient + ",";
            }

            SendTCPMessage(string.Format("NOTICE {0} :{1}", recipient_list.TrimEnd(','), message));
        }

        /// <summary>
        /// Sends a CTCP command and optional message to a nick or channel
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="command"></param>
        /// <param name="message"></param>
        public void SendCTCP(string recipient, string command, string message = "")
        {
            if (message != string.Empty)
            {
                message = " " + message;
            }
            SendTCPMessage(string.Format("PRIVMSG {0} :\u0001{1}{2}\u0001", recipient, command, message));
        }

        public void SendCTCP(List<string> recipients, string command, string message)
        {
            string recipient_list = string.Empty;
            foreach (string recipient in recipients)
            {
                recipient_list += recipient + ",";
            }
            if (message != string.Empty)
            {
                message = " " + message;
            }
            SendTCPMessage(string.Format("PRIVMSG {0} :\u0001{1}{2}\u0001", recipient_list.TrimEnd(','), command, message));
        }

        /// <summary>
        /// Sends the connection password
        /// </summary>
        /// <param name="password"></param>
        public void SendPassword(string password)
        {
            SendTCPMessage(string.Format("PASSWORD {0}", password));
        }

        /// <summary>
        /// Sends a Nick command to set the nickname
        /// </summary>
        /// <param name="nick"></param>
        public void SendNick(string nick)
        {
            SendTCPMessage(string.Format("NICK {0}", nick));
        }

        /// <summary>
        /// Sends the User command to set a user
        /// </summary>
        /// <param name="user"></param>
        public void SendUser(string username, string hostname, string servername, string realname)
        {
            SendTCPMessage(string.Format("USER {0} {1} {2} :{3}", username, hostname, servername, realname));
        }

        /// <summary>
        /// Sends the Oper command to authorize the client as a newtork Oper
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void SendOper(string username, string password)
        {
            SendTCPMessage(string.Format("OPER {0} {1}", username, password));
        }

        /// <summary>
        /// Sends a Quit command to end the client session
        /// </summary>
        /// <param name="message"></param>
        public void SendQuit()
        {
            SendTCPMessage("QUIT");
        }

        public void SendQuit(string message)
        {
            SendTCPMessage(string.Format("QUIT :{0}", message));
        }

        /// <summary>
        /// Sends a Join command to join a channel
        /// </summary>
        /// <param name="channel"></param>
        public void SendJoin(string channel, string key = "")
        {
            string message = string.Empty;
            message = (key != string.Empty) ? string.Format("{0}; {1}", channel, key) : channel;
            SendTCPMessage(string.Format("JOIN {0}", message));
        }

        public void SendJoin(List<string> channels, List<string> keys)
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
        public void SendPart(string channel)
        {
            SendTCPMessage(string.Format("PART {0}", channel));
        }

        public void SendPart(List<string> channels)
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
        public void SendMode(string channel, ChannelModeInfo modeInfo)
        {
            string mode_set = modeInfo.Set ? "+" : "-";
            SendTCPMessage(string.Format("MODE {0} {1} {2}", channel, mode_set + modeInfo.Mode.ToString(), modeInfo.Parameter));
        }

        public void SendMode(string channel, List<ChannelModeInfo> modeInfos)
        {
            foreach (ChannelModeInfo modeInfo in modeInfos)
            {
                SendMode(channel, modeInfo);
            }
        }
        public void SendMode(string nick, UserModeInfo modeInfo)
        {
            string mode_set = modeInfo.Set ? "+" : "-";
            SendTCPMessage(string.Format("MODE {0} {1}", nick, mode_set + modeInfo.Mode.ToString()));
        }

        public void SendMode(string nick, List<UserModeInfo> modeInfos)
        {
            foreach (UserModeInfo modeInfo in modeInfos)
            {
                SendMode(nick, modeInfo);
            }
        }

        /// <summary>
        /// Sends a Topic command to change the channels topic or view the current one
        /// </summary>
        /// <param name="channel"></param>
        public void SendTopic(string channel)
        {
            SendTCPMessage(string.Format("TOPIC {0}", channel));
        }

        public void SendTopic(string channel, string topic)
        {
            SendTCPMessage(string.Format("TOPIC {0} :{1}", channel, topic));
        }

        /// <summary>
        /// Sends a Names command to get a list of visible users
        /// </summary>
        public void SendNames()
        {
            SendTCPMessage("NAMES");
        }

        public void SendNames(string channel)
        {
            SendTCPMessage(string.Format("NAMES {0}", channel));
        }

        public void SendNames(List<string> channels)
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
        public void SendList()
        {
            SendTCPMessage("LIST");
        }

        public void SendList(string channel)
        {
            SendTCPMessage(string.Format("LIST {0}", channel));
        }

        public void SendList(List<string> channels)
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
        public void SendInvite(string channel, string nick)
        {
            SendTCPMessage(string.Format("INVITE {0} {1}", nick, channel));
        }

        /// <summary>
        /// Sends a Kick command to remove a user from a channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="nick"></param>
        public void SendKick(string channel, string nick)
        {
            SendTCPMessage(string.Format("KICK {0} {1}", channel, nick));
        }

        public void SendKick(string channel, string nick, string reason)
        {
            SendTCPMessage(string.Format("KICK {0} {1} :{2}", channel, nick, reason));
        }

        /// <summary>
        /// Sends a Version command to the server to get a Version reply
        /// </summary>
        /// <param name="server"></param>
        public void SendVersion(string server)
        {
            SendTCPMessage(string.Format("VERSION {0}", server));
        }

        /// <summary>
        /// Sends a Stats command to view Server information and statistics
        /// </summary>
        /// <param name="stat"></param>
        public void SendStats(ServerStat stat)
        {
            SendTCPMessage(string.Format("STATS {0}", stat.ToString()));
        }

        public void SendStats(ServerStat stat, string parameter)
        {
            SendTCPMessage(string.Format("STATS {0} {1}", stat.ToString(), parameter));
        }

        /// <summary>
        /// Sends a Links command to list all servers matching a mask
        /// </summary>
        /// <param name="mask"></param>
        public void SendLinks(string mask)
        {
            SendTCPMessage(string.Format("LINKS {0}", mask));
        }

        public void SendLinks(string server, string mask)
        {
            SendTCPMessage(string.Format("LINKS {0} {1}", mask, server));
        }

        /// <summary>
        /// Sends a Time command to query the local server time
        /// </summary>
        public void SendTime()
        {
            SendTCPMessage("TIME");
        }

        public void SendTime(string server)
        {
            SendTCPMessage(string.Format("TIME {0}", server));
        }

        /// <summary>
        /// Senda a Connect command to have the server try to connect to another server
        /// </summary>
        /// <param name="server"></param>
        public void SendConnect(string server)
        {
            SendTCPMessage(string.Format("CONNECT {0}", server));
        }

        public void SendConnect(string server, string originator, int port)
        {
            SendTCPMessage(string.Format("CONNECT {0} {1} {2}", originator, port, server));
        }

        /// <summary>
        /// Sends a Trace command to find the route to the target (nick or server)
        /// </summary>
        /// <param name="target"></param>
        public void SendTrace(string target)
        {
            SendTCPMessage(string.Format("TRACE {0}", target));
        }

        /// <summary>
        /// Sends an Admin command to get the name of the server Administrator
        /// </summary>
        public void SendAdmin()
        {
            SendTCPMessage("ADMIN");
        }

        public void SendAdmin(string host)
        {
            SendTCPMessage(string.Format("ADMIN {0}", host));
        }

        /// <summary>
        /// Sends an Info command for a specific server or nick
        /// </summary>
        /// <param name="host"></param>
        public void SendInfo(string host)
        {
            SendTCPMessage(string.Format("INFO {0}", host));
        }

        /// <summary>
        /// Sends a Who command to list all public users or matching a mask
        /// </summary>
        public void SendWho()
        {
            SendTCPMessage("WHO");
        }

        public void SendWho(string host, bool ops = false)
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
        public void SendWhois(string nick)
        {
            SendTCPMessage(string.Format("WHOIS {0}", nick));
        }

        public void SendWhois(string nick, string server)
        {
            SendTCPMessage(string.Format("WHOIS {0} {1}", server, nick));
        }

        /// <summary>
        /// Sends a Whowas command to get the nick history of a user
        /// </summary>
        /// <param name="nick"></param>
        public void SendWhowas(string nick)
        {
            SendTCPMessage(string.Format("WHOIS {0}", nick));
        }

        public void SendWhowas(string nick, int entries)
        {
            SendTCPMessage(string.Format("WHOIS {0} {1}", nick, entries));
        }

        public void SendWhowas(string nick, int entries, string server)
        {
            SendTCPMessage(string.Format("WHOIS {0} {1} {2}", nick, entries, server));
        }

        /// <summary>
        /// Sends a Kill command to disconnect a nick
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="comment"></param>
        public void SendKill(string nick, string comment)
        {
            SendTCPMessage(string.Format("KILL {0} {1}", nick, comment));
        }

        /// <summary>
        /// Sends a Ping command to the recipient
        /// </summary>
        /// <param name="recipient"></param>
        public void SendPing(string recipient)
        {
            SendTCPMessage(string.Format("PING {0}", recipient));
        }

        /// <summary>
        /// Sends a PONG response to respond to a Ping
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="recipient"></param>
        public void SendPong()
        {
            SendTCPMessage("PONG");
        }

        public void SendPong(string message)
        {
            SendTCPMessage(string.Format("PONG {0}", message));
        }

        public void SendPong(string sender, string recipient)
        {
            SendTCPMessage(string.Format("PONG {0} {1}", sender, recipient));
        }


        /// <summary>
        /// Sends an Away command to unset away status
        /// </summary>
        public void SendAway()
        {
            SendTCPMessage("AWAY");
        }

        /// <summary>
        /// Sends an Away comand to set away status with auto-reply message
        /// </summary>
        /// <param name="message"></param>
        public void SendAway(string message)
        {
            SendTCPMessage(string.Format("AWAY {0}", message));
        }

        /// <summary>
        /// Sends a Rehash command to the server to reload it's configuration file
        /// </summary>
        public void SendRehash()
        {
            SendTCPMessage("REHASH");
        }

        /// <summary>
        /// Sends a Restart command to the server to restart
        /// </summary>
        public void SendRestart()
        {
            SendTCPMessage("RESTART");
        }

        /// <summary>
        /// Sends a Summon command to summon a nick to the server
        /// </summary>
        /// <param name="nick"></param>
        public void SendSummon()
        {
            SendTCPMessage("SUMMON");
        }

        public void SendSummon(string nick)
        {
            SendTCPMessage(string.Format("SUMMON {0}", nick));
        }

        public void SendSummon(string nick, string host)
        {
            SendTCPMessage(string.Format("SUMMON {0} {1}", nick, host));
        }

        /// <summary>
        /// Sends a Users command to get a list of Users from a server
        /// </summary>
        /// <param name="server"></param>
        public void SendUsers(string server)
        {
            SendTCPMessage(string.Format("USERS {0}", server));
        }

        /// <summary>
        /// Sends a Wallops command which sends a message to all connected ops
        /// </summary>
        /// <param name="message"></param>
        public void SendWallops(string message)
        {
            SendTCPMessage(string.Format("WALLOPS :{0}", message));
        }

        /// <summary>
        /// Sends an Userhost command to up to 5 nicknames to return information about each nick
        /// </summary>
        /// <param name="nicks"></param>
        public void SendUserhost(List<string> nicks)
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
        public void SendIson(List<string> nicks)
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
