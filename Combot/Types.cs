using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net;

namespace Combot
{
    public enum ErrorType
    {
        Bot = 0,
        TCP = 1,
        IRC = 2,
        Framework = 3
    }

    public class BotError
    {
        public ErrorType Type { get; set; }
        public string Message { get; set; }
    }
}
