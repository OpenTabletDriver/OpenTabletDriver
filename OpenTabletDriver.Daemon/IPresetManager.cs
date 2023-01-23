using System.Collections.Generic;
using OpenTabletDriver.Daemon.Contracts;

namespace OpenTabletDriver.Daemon
{
    public interface IPresetManager
    {
        IReadOnlyCollection<string> GetPresets();
        Preset? LoadPreset(string name);
        void Save(string name, Settings settings);
    }
}
