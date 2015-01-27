using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combot
{
    public class Command
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public List<CommandArgument> Arguments { get; set; }
    }

    public class CommandArgument
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Required { get; set; }
    }
}
