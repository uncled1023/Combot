using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combot.Modules.Plugins
{
    public enum RelayType
    {
        Message,
        Notice,
        CTCP,
        Join,
        Part,
        Quit,
        Kick,
        Invite,
        Mode,
        Topic,
        Nick
    }
}
