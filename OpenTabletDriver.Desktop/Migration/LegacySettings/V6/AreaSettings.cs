using System;
using Newtonsoft.Json;

namespace OpenTabletDriver.Desktop.Migration.LegacySettings.V6
{
    [JsonObject]
    internal class AreaSettings : IMigrate<AngledArea>
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

        public AngledArea Migrate(IServiceProvider serviceProvider)
        {
            return new AngledArea
            {
                Width = Width,
                Height = Height,
                XPosition = X,
                YPosition = Y,
                Rotation = Rotation
            };
        }
    }
}
