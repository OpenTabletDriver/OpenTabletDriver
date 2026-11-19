using System.Numerics;
using Newtonsoft.Json;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class AreaSettings : ViewModel
    {
        private float width, height, x, y, rotation;

        [JsonProperty(nameof(Width))]
        public float Width
        {
            set => this.RaiseAndSetIfChanged(ref this.width, value);
            get => this.width;
        }

        [JsonProperty(nameof(Height))]
        public float Height
        {
            set => this.RaiseAndSetIfChanged(ref this.height, value);
            get => this.height;
        }

        [JsonProperty(nameof(X))]
        public float X
        {
            set => this.RaiseAndSetIfChanged(ref this.x, value);
            get => this.x;
        }

        [JsonProperty(nameof(Y))]
        public float Y
        {
            set => this.RaiseAndSetIfChanged(ref this.y, value);
            get => this.y;
        }

        [JsonProperty(nameof(Rotation))]
        public float Rotation
        {
            set => this.RaiseAndSetIfChanged(ref this.rotation, value);
            get => this.rotation;
        }

        [JsonIgnore]
        public Area Area
        {
            set
            {
                Width = value.Width;
                Height = value.Height;
                X = value.Position.X;
                Y = value.Position.Y;
                Rotation = value.Rotation;
            }
            get => new(this.Width, this.Height, new Vector2(this.X, this.Y), this.Rotation);
        }

        public static AreaSettings GetDefaults(DigitizerSpecifications digitizer) =>
            getDefaults(digitizer.Width, digitizer.Height);

        public static AreaSettings GetDefaults(IDisplay display) =>
            getDefaults(display.Width, display.Height);

        private static AreaSettings getDefaults(float width, float height) =>
            new()
            {
                Width = width,
                Height = height,
                X = width / 2,
                Y = height / 2,
            };
    }
}
