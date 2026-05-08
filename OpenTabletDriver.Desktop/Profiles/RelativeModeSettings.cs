using System;
using System.Numerics;
using Newtonsoft.Json;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class RelativeModeSettings : ViewModel
    {
        [JsonProperty(nameof(XSensitivity))]
        public float XSensitivity
        {
            set => RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(YSensitivity))]
        public float YSensitivity
        {
            set => RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty(nameof(RelativeRotation))]
        public float RelativeRotation
        {
            set => RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonProperty("RelativeResetDelay")]
        public TimeSpan ResetTime
        {
            set => RaiseAndSetIfChanged(ref field, value);
            get;
        }

        [JsonIgnore]
        public Vector2 Sensitivity
        {
            set
            {
                XSensitivity = value.X;
                YSensitivity = value.Y;
            }
            get => new Vector2(XSensitivity, YSensitivity);
        }

        public static RelativeModeSettings GetDefaults()
        {
            return new RelativeModeSettings
            {
                XSensitivity = 10,
                YSensitivity = 10,
                RelativeRotation = 0,
                ResetTime = TimeSpan.FromMilliseconds(100)
            };
        }
    }
}
