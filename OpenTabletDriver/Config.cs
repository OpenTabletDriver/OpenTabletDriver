using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Numerics;
using Newtonsoft.Json;
using OpenTabletDriver.Binding;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver
{
    public class Config : Notifier
    {
        public Config()
        {
        }

        private string _currentProfile;

        #region General Settings

        [JsonProperty("CurrentProfile")]
        public string CurrentProfile
        {
            set => RaiseAndSetIfChanged(ref _currentProfile, value != "{Disable}" ? value : null);
            get => _currentProfile;
        }

        #endregion

        #region Json Serialization

        public static Config Deserialize(FileInfo file)
        {
            using (var stream = file.OpenRead())
            using (var sr = new StreamReader(stream))
            {
                var str = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<Config>(str);
            }
        }

        public void Serialize(FileInfo file)
        {
            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(file.FullName, str);
        }

        #endregion

        #region Defaults

        public static readonly Config Defaults = new Config
        {
            CurrentProfile = "Default",
        };

        #endregion
    }
}