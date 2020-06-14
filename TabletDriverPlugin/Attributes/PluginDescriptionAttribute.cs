namespace TabletDriverPlugin.Attributes
{
    public class PluginDescriptionAttribute
    {
        public PluginDescriptionAttribute(string description)
        {
            Description = description;
        }

        public string Description { private set; get; }
    }
}