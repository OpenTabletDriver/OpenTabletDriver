namespace OpenTabletDriver.Daemon.Contracts.Persistence
{
    public class Preset
    {
        public Preset(string name, Settings settings)
        {
            Name = name;
            Settings = settings;
        }

        public string Name { get; }
        public Settings Settings { get; }
    }
}
