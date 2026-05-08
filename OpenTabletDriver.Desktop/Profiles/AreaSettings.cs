using System.Numerics;
using Newtonsoft.Json;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class AreaSettings : ViewModel
    {
        [JsonProperty(nameof(Width))]
        public float Width
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(Height))]
        public float Height
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(X))]
        public float X
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(Y))]
        public float Y
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(Rotation))]
        public float Rotation
        {
            set => this.RaiseAndSetIfChanged(ref field, value);
            get;
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
            get => new Area
            {
                Width = this.Width,
                Height = this.Height,
                Position = new Vector2(this.X, this.Y),
                Rotation = this.Rotation
            };
        }

        public static AreaSettings GetDefaults(DigitizerSpecifications digitizer)
        {
            return new AreaSettings
            {
                Width = digitizer.Width,
                Height = digitizer.Height,
                X = digitizer.Width / 2,
                Y = digitizer.Height / 2
            };
        }

        public static AreaSettings GetDefaults(IDisplay display)
        {
            return new AreaSettings
            {
                Width = display.Width,
                Height = display.Height,
                X = display.Width / 2,
                Y = display.Height / 2
            };
        }
    }
}
