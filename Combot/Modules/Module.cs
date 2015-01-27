using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combot.Modules
{
    abstract internal class Module
    {
        abstract public List<Command> Commands { get; set; }

        abstract internal void Initialize() { }
    }
}
