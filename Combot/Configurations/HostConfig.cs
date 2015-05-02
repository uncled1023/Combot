namespace Combot.Configurations
{
    public class HostConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }

        public HostConfig()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Host = string.Empty;
            Port = 0;
        }
    }
}
