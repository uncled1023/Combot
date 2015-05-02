namespace Combot.Modules
{
    public class Option
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public object Value { get; set; }

        public Option()
        {
            SetDefaults();
        }

        public void SetDefaults()
        {
            Name = string.Empty;
            Description = string.Empty;
            Value = null;
        }

        public void Copy(Option option)
        {
            Name = option.Name;
            Description = option.Description;
            Value = option.Value;
        }
    }
}