using System.Collections.Generic;
using System.Threading;

namespace Combot.IRCServices.Messaging
{
    public class GetReply
    {
        public ManualResetEventSlim Ready = new ManualResetEventSlim(false);
        public string Match = string.Empty;
        public IRCReplyCode Reply = new IRCReplyCode();
        public ServerReplyMessage Result = new ServerReplyMessage();
    }
}
