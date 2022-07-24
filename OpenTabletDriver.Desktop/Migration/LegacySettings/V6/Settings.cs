using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.Desktop.Migration.LegacySettings.V6
{
    public class Settings : NotifyPropertyChanged
    {
        [JsonProperty("Profiles")]
        public Collection<Profile>? Profiles { set; get; }

        [JsonProperty("LockUsableAreaDisplay")]
        public bool LockUsableAreaDisplay { set; get; }

        [JsonProperty("LockUsableAreaTablet")]
        public bool LockUsableAreaTablet { set; get; }

        [JsonProperty("Tools")]
        public Collection<PluginSettings>? Tools { set; get; }

        public bool IsValid()
        {
            return Profiles != null
                   && Tools != null
                   && Profiles.All(p => p.AbsoluteModeSettings != null && p.RelativeModeSettings != null);
        }
    }
}
