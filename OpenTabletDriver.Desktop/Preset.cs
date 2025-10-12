using System;

namespace OpenTabletDriver.Desktop
{
    public class Preset(string name, Settings settings)
    {
        public string Name { get; } = name;
        public Settings Settings { get; } = settings;

        [Obsolete("Use the Settings property instead.")]
        public Settings GetSettings() => Settings;
    }
}
