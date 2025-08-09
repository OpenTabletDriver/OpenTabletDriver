using System;
using System.Numerics;
using Newtonsoft.Json;

namespace OpenTabletDriver.Desktop.Profiles
{
    public class RelativeModeSettings : ViewModel
    {
        private float xS, yS, relRot;
        private TimeSpan rT;

        [JsonProperty("XSensitivity")]
        public float XSensitivity
        {
            set => RaiseAndSetIfChanged(ref this.xS, value);
            get => this.xS;
        }

        [JsonProperty("YSensitivity")]
        public float YSensitivity
        {
            set => RaiseAndSetIfChanged(ref this.yS, value);
            get => this.yS;
        }

        [JsonProperty("RelativeRotation")]
        public float RelativeRotation
        {
            set => RaiseAndSetIfChanged(ref this.relRot, value);
            get => this.relRot;
        }

        [JsonProperty("RelativeResetDelay")]
        public TimeSpan ResetTime
        {
            set => RaiseAndSetIfChanged(ref this.rT, value);
            get => this.rT;
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
