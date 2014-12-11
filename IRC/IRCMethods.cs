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
        protected void SendPrivMessage(Nick nick, string message)
        {
            SendTCPMessage(string.Format("PRIVMSG {0} :{1}", nick, message));
        }

        protected void SendPrivMessage(List<Nick> nicks, string message)
        {
            string nick_list = string.Empty;
            foreach (Nick nick in nicks)
            {
                nick_list += nick.Nickname + ",";
            }

            SendTCPMessage(string.Format("PRIVMSG {0} :{1}", nick_list.TrimEnd(','), message));
        }

        protected void SendPrivMessage(Channel channel, string message)
        {
            SendTCPMessage(string.Format("PRIVMSG {0} :{1}", channel.Name, message));
        }

        protected void SendPrivMessage(List<Channel> channels, string message)
        {
            string channel_list = string.Empty;
            foreach (Channel channel in channels)
            {
                channel_list += channel.Name + ",";
            }

            SendTCPMessage(string.Format("PRIVMSG {0} :{1}", channel_list.TrimEnd(','), message));
        }

        /// <summary>
        /// Sends a Notice to either a nick or channel
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="message"></param>
        protected void SendNotice(Nick nick, string message)
        {
            SendTCPMessage(string.Format("NOTICE {0} :{1}", nick, message));
        }

        protected void SendNotice(List<Nick> nicks, string message)
        {
            string nick_list = string.Empty;
            foreach (Nick nick in nicks)
            {
                nick_list += nick.Nickname + ",";
            }

            SendTCPMessage(string.Format("NOTICE {0} :{1}", nick_list.TrimEnd(','), message));
        }

        protected void SendNotice(Channel channel, string message)
        {
            SendTCPMessage(string.Format("NOTICE {0} :{1}", channel.Name, message));
        }

        protected void SendNotice(List<Channel> channels, string message)
        {
            string channel_list = string.Empty;
            foreach (Channel channel in channels)
            {
                channel_list += channel.Name + ",";
            }

            SendTCPMessage(string.Format("NOTICE {0} :{1}", channel_list.TrimEnd(','), message));
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
        protected void SendJoin(Channel channel)
        {
            string message = string.Empty;
            message = (channel.Key != string.Empty) ? string.Format("{0}; {1}", channel.Name, channel.Key) : channel.Name;
            SendTCPMessage(string.Format("JOIN {0}", message));
        }

        protected void SendJoin(List<Channel> channels)
        {
            string message = string.Empty;
            string channel_string = string.Empty;
            string key_string = string.Empty;

            foreach (Channel channel in channels)
            {                
                channel_string += channel.Name + ",";
                if (channel.Key != string.Empty)
                {
                    key_string += channel.Key + ",";
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
        protected void SendPart(Channel channel)
        {
            SendTCPMessage(string.Format("PART {0}", channel.Name));
        }

        protected void SendPart(List<Channel> channels)
        {
            string channel_list = string.Empty;
            foreach (Channel channel in channels)
            {
                channel_list += channel.Name + ",";
            }

            SendTCPMessage(string.Format("PART {0}", channel_list.TrimEnd(',')));
        }


        /// <summary>
        /// Sends a Mode command for either a channel mode or user mode
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="mode"></param>
        protected void SendMode(Channel channel, ChannelModeInfo modeInfo)
        {
            string mode_set = modeInfo.Set ? "+" : "-";
            SendTCPMessage(string.Format("MODE {0} {1} {2}", channel.Name, mode_set + modeInfo.Mode.ToString(), modeInfo.Parameter));
        }

        protected void SendMode(Channel channel, List<ChannelModeInfo> modeInfos)
        {
            foreach (ChannelModeInfo modeInfo in modeInfos)
            {
                SendMode(channel, modeInfo);
            }
        }
        protected void SendMode(Nick nick, UserModeInfo modeInfo)
        {
            string mode_set = modeInfo.Set ? "+" : "-";
            SendTCPMessage(string.Format("MODE {0} {1} {2}", nick.Nickname, mode_set + modeInfo.Mode.ToString(), modeInfo.Parameter));
        }

        protected void SendMode(Nick nick, List<UserModeInfo> modeInfos)
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
        protected void SendTopic(Channel channel)
        {
            SendTCPMessage(string.Format("TOPIC {0}", channel.Name));
        }

        protected void SendTopic(Channel channel, string topic)
        {
            SendTCPMessage(string.Format("TOPIC {0} :{1}", channel.Name, topic));
        }

        /// <summary>
        /// Sends a Names command to get a list of visible users
        /// </summary>
        protected void SendNames()
        {
            SendTCPMessage("NAMES");
        }

        protected void SendNames(Channel channel)
        {
            SendTCPMessage(string.Format("NAMES ", channel.Name));
        }

        protected void SendNames(List<Channel> channels)
        {
            string channel_list = string.Empty;
            foreach(Channel channel in channels)
            {
                channel_list += channel.Name + ",";
            }
            SendTCPMessage(string.Format("NAMES ", channel_list.TrimEnd(',')));
        }

        /// <summary>
        /// Sends a List command to get the topic of channels
        /// </summary>
        protected void SendList()
        {
            SendTCPMessage("LIST");
        }

        protected void SendList(Channel channel)
        {
            SendTCPMessage(string.Format("LIST ", channel.Name));
        }

        protected void SendList(List<Channel> channels)
        {
            string channel_list = string.Empty;
            foreach (Channel channel in channels)
            {
                channel_list += channel.Name + ",";
            }
            SendTCPMessage(string.Format("LIST ", channel_list.TrimEnd(',')));
        }

        /// <summary>
        /// Sends an Invite command that invites the specified nick to the channel 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="nick"></param>
        protected void SendInvite(Channel channel, Nick nick)
        {
            SendTCPMessage(string.Format("INVITE {0} {1}", nick.Nickname, channel.Name));
        }

        /// <summary>
        /// Sends a Kick command to remove a user from a channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="nick"></param>
        protected void SendKick(Channel channel, Nick nick)
        {
            SendTCPMessage(string.Format("KICK {0} {1}", channel.Name, nick.Nickname));
        }

        protected void SendKick(Channel channel, Nick nick, string reason)
        {
            SendTCPMessage(string.Format("KICK {0} {1} :{2}", channel.Name, nick.Nickname, reason));
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

        protected void SendLinks(string server)
        {
            SendTCPMessage(string.Format("LINKS {0}", server));
        }

        protected void SendLinks(List<string> servers)
        {
            foreach (
        }
    }
}
