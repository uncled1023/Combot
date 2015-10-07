using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Combot
{
    public class SpamSession
    {
        public int CurrentCount { get; set; }
        public DateTime LastInstance { get; set; }

        public SpamSession()
        {
            SetDefaults();
        }

        private void SetDefaults()
        {
            CurrentCount = 1;
            LastInstance = DateTime.Now;
        }
    }
}
