namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginInfo
    {
        public string Name { get; init; }
        public string Path { get; init; }
        public PluginState State { get; set; } = PluginState.Normal;
        public PluginForm Form { get; init; }
    }
}