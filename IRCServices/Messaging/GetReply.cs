using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Combot.IRCServices.Messaging
{
    public class GetReply
    {
        public bool Reattach = true;
        public bool Result = false;
        public ManualResetEventSlim Ready = new ManualResetEventSlim(false);
        public List<IRCReplyCode> Replies = new List<IRCReplyCode>();
        public List<IRCErrorCode> Errors = new List<IRCErrorCode>();
    }
}
