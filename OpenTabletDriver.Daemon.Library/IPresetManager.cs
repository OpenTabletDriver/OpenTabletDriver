using System.Collections.Generic;
using OpenTabletDriver.Daemon.Contracts.Persistence;

namespace OpenTabletDriver.Daemon.Library
{
    public interface IPresetManager
    {
        IReadOnlyCollection<string> GetPresets();
        Preset? LoadPreset(string name);
        void Save(string name, Settings settings);
    }
}
