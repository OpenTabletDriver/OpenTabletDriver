using Newtonsoft.Json;

namespace OpenTabletDriver.Desktop.Migration.LegacySettings.V6
{
    public class AreaSettings
    {
        [JsonProperty("Width")]
        public float Width { set; get; }

        [JsonProperty("Height")]
        public float Height { set; get; }

        [JsonProperty("X")]
        public float X { set; get; }

        [JsonProperty("Y")]
        public float Y { set; get; }

        [JsonProperty("Rotation")]
        public float Rotation { set; get; }
    }
}
