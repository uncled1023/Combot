using Combot.IRCServices.Messaging;

namespace Combot.Modules.Plugins
{
    public class Invite : Module
    {
        public override void Initialize()
        {
            Bot.IRC.Message.InviteChannelEvent += HandleInvite;
        }

        private void HandleInvite(object sender, InviteChannelInfo inviteInfo)
        {
            if (!Bot.ServerConfig.NickBlacklist.Contains(inviteInfo.Requester.Nickname)
                && !NickBlacklist.Contains(inviteInfo.Requester.Nickname))
            {
                if (!Bot.ServerConfig.ChannelBlacklist.Contains(inviteInfo.Channel) && !ChannelBlacklist.Contains(inviteInfo.Channel))
                {
                    Bot.IRC.Command.SendJoin(inviteInfo.Channel);
                    string helpMessage = string.Empty;
                    if (Bot.Modules.Exists(module => module.Commands.Exists(cmd => cmd.Triggers.Contains("help") && cmd.Enabled)))
                    {
                        helpMessage = string.Format("  For more information on what I can do, just type: {0}help", Bot.ServerConfig.CommandPrefix);
                    }
                    Bot.IRC.Command.SendPrivateMessage(inviteInfo.Channel, string.Format("{0} has invited me to this channel.  If you would like me to leave, just kick me.{1}", inviteInfo.Requester.Nickname, helpMessage));
                }
                else
                {
                    Bot.IRC.Command.SendNotice(inviteInfo.Requester.Nickname, "I am unable to join that channel.");
                }
            }
        }
    }
}