using System.Collections.Generic;

namespace OpenTabletDriver.Desktop
{
    public interface IPresetManager
    {
        IReadOnlyCollection<string> GetPresets();
        Preset LoadPreset(string name);
        void Save(string name, Settings settings);
    }
}
