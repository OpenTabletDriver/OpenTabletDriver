using System;
using Newtonsoft.Json;

namespace OpenTabletDriver.Desktop.Migration.LegacySettings.V6
{
    internal class RelativeModeSettings
    {
        [JsonProperty("XSensitivity")]
        public float XSensitivity { set; get; }

        [JsonProperty("YSensitivity")]
        public float YSensitivity { set; get; }

        [JsonProperty("RelativeRotation")]
        public float RelativeRotation { set; get; }

        [JsonProperty("RelativeResetDelay")]
        public TimeSpan ResetTime { set; get; }
    }
}
