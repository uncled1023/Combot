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
        protected void SendMode(Channel channel, ChannelMode mode)
        {

        }
    }
}
