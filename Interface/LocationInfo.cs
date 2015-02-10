namespace Interface
{
    public class LocationInfo
    {
        public string Name { get; set; }

        public LocationInfo()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Name = string.Empty;
        }
    }
}