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

        // ------------------- //
        // Internal Functions  //
        // ------------------- //

        /// <summary>
        /// Sends a private message to a nick or channel
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="message"></param>
        protected void SendPrivMessage(string recipient, string message)
        {
            SendTCPMessage(string.Format("PRIVMSG {0} :{1}", recipient, message));
        }

        protected void SendPrivMessage(List<string> recipients, string message)
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
        protected void SendNotice(string recipient, string message)
        {
            SendTCPMessage(string.Format("NOTICE {0} :{1}", recipient, message));
        }

        protected void SendNotice(List<string> recipients, string message)
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
        protected void SendPassword(string password)
        {
            SendTCPMessage(password);
        }

        /// <summary>
        /// Sends a Nick command to set the nickname
        /// </summary>
        /// <param name="nick"></param>
        protected void SendNick(string nick)
        {
            SendTCPMessage(nick);
        }

        /// <summary>
        /// Sends the User command to set a user
        /// </summary>
        /// <param name="user"></param>
        protected void SendUser(string username, string hostname, string servername, string realname)
        {
            SendTCPMessage(string.Format("USER {0} {1} {2} :{3}", username, hostname, servername, realname));
        }

        /// <summary>
        /// Sends the Oper command to authorize the client as a newtork Oper
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        protected void SendOper(string username, string password)
        {
            SendTCPMessage(string.Format("OPER {0} {1}", username, password));
        }

        /// <summary>
        /// Sends a Quit command to end the client session
        /// </summary>
        /// <param name="message"></param>
        protected void SendQuit()
        {
            SendTCPMessage("QUIT");
        }

        protected void SendQuit(string message)
        {
            SendTCPMessage(string.Format("QUIT :{0}", message));
        }

        /// <summary>
        /// Sends a Join command to join a channel
        /// </summary>
        /// <param name="channel"></param>
        protected void SendJoin(string channel, string key = "")
        {
            string message = string.Empty;
            message = (key != string.Empty) ? string.Format("{0}; {1}", channel, key) : channel;
            SendTCPMessage(string.Format("JOIN {0}", message));
        }

        protected void SendJoin(List<string> channels, List<string> keys)
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
        protected void SendPart(string channel)
        {
            SendTCPMessage(string.Format("PART {0}", channel));
        }

        protected void SendPart(List<string> channels)
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
        protected void SendMode(string channel, ChannelModeInfo modeInfo)
        {
            string mode_set = modeInfo.Set ? "+" : "-";
            SendTCPMessage(string.Format("MODE {0} {1} {2}", channel, mode_set + modeInfo.Mode.ToString(), modeInfo.Parameter));
        }

        protected void SendMode(string channel, List<ChannelModeInfo> modeInfos)
        {
            foreach (ChannelModeInfo modeInfo in modeInfos)
            {
                SendMode(channel, modeInfo);
            }
        }
        protected void SendMode(string nick, UserModeInfo modeInfo)
        {
            string mode_set = modeInfo.Set ? "+" : "-";
            SendTCPMessage(string.Format("MODE {0} {1} {2}", nick, mode_set + modeInfo.Mode.ToString(), modeInfo.Parameter));
        }

        protected void SendMode(string nick, List<UserModeInfo> modeInfos)
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
        protected void SendTopic(string channel)
        {
            SendTCPMessage(string.Format("TOPIC {0}", channel));
        }

        protected void SendTopic(string channel, string topic)
        {
            SendTCPMessage(string.Format("TOPIC {0} :{1}", channel, topic));
        }

        /// <summary>
        /// Sends a Names command to get a list of visible users
        /// </summary>
        protected void SendNames()
        {
            SendTCPMessage("NAMES");
        }

        protected void SendNames(string channel)
        {
            SendTCPMessage(string.Format("NAMES {0}", channel));
        }

        protected void SendNames(List<string> channels)
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
        protected void SendList()
        {
            SendTCPMessage("LIST");
        }

        protected void SendList(string channel)
        {
            SendTCPMessage(string.Format("LIST {0}", channel));
        }

        protected void SendList(List<string> channels)
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
        protected void SendInvite(string channel, string nick)
        {
            SendTCPMessage(string.Format("INVITE {0} {1}", nick, channel));
        }

        /// <summary>
        /// Sends a Kick command to remove a user from a channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="nick"></param>
        protected void SendKick(string channel, string nick)
        {
            SendTCPMessage(string.Format("KICK {0} {1}", channel, nick));
        }

        protected void SendKick(string channel, string nick, string reason)
        {
            SendTCPMessage(string.Format("KICK {0} {1} :{2}", channel, nick, reason));
        }

        /// <summary>
        /// Sends a Version command to the server to get a Version reply
        /// </summary>
        /// <param name="server"></param>
        protected void SendVersion(string server)
        {
            SendTCPMessage(string.Format("VERSION {0}", server));
        }

        /// <summary>
        /// Sends a Stats command to view Server information and statistics
        /// </summary>
        /// <param name="stat"></param>
        protected void SendStats(ServerStat stat)
        {
            SendTCPMessage(string.Format("STATS {0}", stat.ToString()));
        }

        protected void SendStats(ServerStat stat, string parameter)
        {
            SendTCPMessage(string.Format("STATS {0} {1}", stat.ToString(), parameter));
        }

        /// <summary>
        /// Sends a Links command to list all servers matching a mask
        /// </summary>
        /// <param name="mask"></param>
        protected void SendLinks(string mask)
        {
            SendTCPMessage(string.Format("LINKS {0}", mask));
        }

        protected void SendLinks(string server, string mask)
        {
            SendTCPMessage(string.Format("LINKS {0} {1}", mask, server));
        }

        /// <summary>
        /// Sends a Time command to query the local server time
        /// </summary>
        protected void SendTime()
        {
            SendTCPMessage("TIME");
        }

        protected void SendTime(string server)
        {
            SendTCPMessage(string.Format("TIME {0}", server));
        }

        /// <summary>
        /// Senda a Connect command to have the server try to connect to another server
        /// </summary>
        /// <param name="server"></param>
        protected void SendConnect(string server)
        {
            SendTCPMessage(string.Format("CONNECT {0}", server));
        }

        protected void SendConnect(string server, string originator, int port)
        {
            SendTCPMessage(string.Format("CONNECT {0} {1} {2}", originator, port, server));
        }


        /// <summary>
        /// Sends a Trace command to find the route to the target (nick or server)
        /// </summary>
        /// <param name="target"></param>
        protected void SendTrace(string target)
        {
            SendTCPMessage(string.Format("TRACE {0}", target));
        }
    }
}
