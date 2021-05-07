using System.Collections.Generic;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop
{
    public class Settings : ViewModel
    {
        private Dictionary<string, string> _activeProfiles = new Dictionary<string, string>();
        private PluginSettingStoreCollection _tools = new PluginSettingStoreCollection();

        /// <summary>
        /// The active profile for tablets
        /// </summary>
        /// <remarks>
        /// The key is a tablet name as defined in the configurations, while the value represents the profile filename under tablet name string's directory
        /// </remarks>
        [JsonProperty("ActiveProfiles")]
        public Dictionary<string, string> ActiveProfiles
        {
            set => RaiseAndSetIfChanged(ref _activeProfiles, value);
            get => _activeProfiles;
        }

        /// <summary>
        /// Tools to run on boot of OTD daemon
        /// </summary>
        [JsonProperty("Tools")]
        public PluginSettingStoreCollection Tools
        {
            set => RaiseAndSetIfChanged(ref _tools, value);
            get => _tools;
        }

        /// <summary>
        /// Retrieve active profile of the given tablet
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns the saved active profile of the tablet, or a default one if an active profile does not exist</returns>
        public Profile DeserializeActiveProfile(TabletHandlerID id)
        {
            var tabletName = Info.Driver.GetTabletState(id)?.Properties.Name;

            if (ActiveProfiles.ContainsKey(tabletName))
            {
                var profileName = ActiveProfiles[tabletName];
                return ProfileSerializer.Deserialize(id, profileName);
            }

            var profile = ProfileSerializer.GetDefaultProfile(id);

            ActiveProfiles.TryAdd(tabletName, profile.ProfileName);
            return profile;
        }
    }
}
