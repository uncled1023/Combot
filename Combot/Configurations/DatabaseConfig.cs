namespace Combot.Configurations
{
    public class DatabaseConfig
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public DatabaseConfig()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Server = "localhost";
            Port = 3306;
            Database = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
        }
    }
}