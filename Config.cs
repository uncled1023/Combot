using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combot
{
    class Config
    {
        private Nick _nick;
        public Nick Nick
        {
            get
            {
                return _nick;
            }

            set
            {
                if (value != _nick)
                {
                    _nick = value;
                }
            }
        }
    }
}
