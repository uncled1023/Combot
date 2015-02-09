using System.Collections.Generic;

namespace Interface
{
    public class BufferInfo
    {
        public string Server { get; set; }
        public string Location { get; set; }
        public List<string> Buffer { get; set; }

        public BufferInfo()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Server = string.Empty;
            Location = string.Empty;
            Buffer = new List<string>();
        }
    }
}