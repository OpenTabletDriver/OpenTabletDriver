using System;
using System.Numerics;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop
{
    public class Profile : ViewModel
    {
        internal const int PenButtonCount = 2;
        internal const int AuxButtonCount = 8;

        private string _profileName, _compatibleDevice;

        private float _dW, _dH, _dX, _dY, _tW, _tH, _tX, _tY, _r, _xS, _yS, _relRot, _tP, _eP; 
        private TimeSpan _rT;
        private bool _lockar, _sizeChanging, _autoHook, _clipping, _areaLimiting, _lockUsableAreaDisplay, _lockUsableAreaTablet;
        private PluginSettingStore _outputMode, _tipButton, _eraserButton;

        private PluginSettingStoreCollection _filters = new PluginSettingStoreCollection(),
            _penButtons = new PluginSettingStoreCollection(),
            _auxButtons = new PluginSettingStoreCollection();

        // General Settings

        [JsonProperty("ProfileName")]
        public string ProfileName
        {
            set => RaiseAndSetIfChanged(ref _profileName, value);
            get => _profileName;
        }

        [JsonProperty("CompatibleDevice")]
        public string CompatibleDevice
        {
            set => RaiseAndSetIfChanged(ref _compatibleDevice, value);
            get => _compatibleDevice;
        }

        [JsonProperty("OutputMode")]
        public PluginSettingStore OutputMode
        {
            set => RaiseAndSetIfChanged(ref _outputMode, value);
            get => _outputMode;
        }

        [JsonProperty("Filters")]
        public PluginSettingStoreCollection Filters
        {
            set => RaiseAndSetIfChanged(ref _filters, value);
            get => _filters;
        }

        [JsonProperty("AutoHook")]
        public bool AutoHook
        {
            set => RaiseAndSetIfChanged(ref _autoHook, value);
            get => _autoHook;
        }

        [JsonProperty("LockUsableAreaDisplay")]
        public bool LockUsableAreaDisplay
        {
            set => this.RaiseAndSetIfChanged(ref _lockUsableAreaDisplay, value);
            get => _lockUsableAreaDisplay;
        }

        [JsonProperty("LockUsableAreaTablet")]
        public bool LockUsableAreaTablet
        {
            set => this.RaiseAndSetIfChanged(ref _lockUsableAreaTablet, value);
            get => _lockUsableAreaTablet;
        }

        // Absolute Mode Settings

        [JsonProperty("DisplayWidth")]
        public float DisplayWidth
        {
            set
            {
                RaiseAndSetIfChanged(ref _dW, value);
                if (LockAspectRatio)
                    TabletHeight = DisplayHeight / DisplayWidth * TabletWidth;
            }
            get => _dW;
        }

        [JsonProperty("DisplayHeight")]
        public float DisplayHeight
        {
            set
            {
                RaiseAndSetIfChanged(ref _dH, value);
                if (LockAspectRatio)
                    TabletWidth = DisplayWidth / DisplayHeight * TabletHeight;
            }
            get => _dH;
        }

        [JsonProperty("DisplayXOffset")]
        public float DisplayX
        {
            set => RaiseAndSetIfChanged(ref _dX, value);
            get => _dX;
        }

        [JsonProperty("DisplayYOffset")]
        public float DisplayY
        {
            set => RaiseAndSetIfChanged(ref _dY, value);
            get => _dY;
        }

        [JsonProperty("TabletWidth")]
        public float TabletWidth
        {
            set
            {
                RaiseAndSetIfChanged(ref _tW, value);
                if (LockAspectRatio && !_sizeChanging)
                {
                    _sizeChanging = true;
                    TabletHeight = DisplayHeight / DisplayWidth * value;
                    _sizeChanging = false;
                }
            }
            get => _tW;
        }

        [JsonProperty("TabletHeight")]
        public float TabletHeight
        {
            set
            {
                RaiseAndSetIfChanged(ref _tH, value);
                if (LockAspectRatio && !_sizeChanging)
                {
                    _sizeChanging = true;
                    TabletWidth = DisplayWidth / DisplayHeight * value;
                    _sizeChanging = false;
                }
            }
            get => _tH;
        }

        [JsonProperty("TabletXOffset")]
        public float TabletX
        {
            set => RaiseAndSetIfChanged(ref _tX, value);
            get => _tX;
        }

        [JsonProperty("TabletYOffset")]
        public float TabletY
        {
            set => RaiseAndSetIfChanged(ref _tY, value);
            get => _tY;
        }

        [JsonProperty("TabletRotation")]
        public float TabletRotation
        {
            set => RaiseAndSetIfChanged(ref _r, value);
            get => _r;
        }

        [JsonProperty("EnableClipping")]
        public bool EnableClipping
        {
            set => RaiseAndSetIfChanged(ref _clipping, value);
            get => _clipping;
        }

        [JsonProperty("EnableAreaLimiting")]
        public bool EnableAreaLimiting
        {
            set => RaiseAndSetIfChanged(ref _areaLimiting, value);
            get => _areaLimiting;
        }

        [JsonProperty("LockAspectRatio")]
        public bool LockAspectRatio
        {
            set
            {
                RaiseAndSetIfChanged(ref _lockar, value);
                if (value)
                    TabletHeight = DisplayHeight / DisplayWidth * TabletWidth;
            }
            get => _lockar;
        }

        // Relative Mode Settings

        [JsonProperty("XSensitivity")]
        public float XSensitivity
        {
            set => RaiseAndSetIfChanged(ref _xS, value);
            get => _xS;
        }

        [JsonProperty("YSensitivity")]
        public float YSensitivity
        {
            set => RaiseAndSetIfChanged(ref _yS, value);
            get => _yS;
        }

        [JsonProperty("RelativeRotation")]
        public float RelativeRotation
        {
            set => RaiseAndSetIfChanged(ref _relRot, value);
            get => _relRot;
        }

        [JsonProperty("RelativeResetDelay")]
        public TimeSpan ResetTime
        {
            set => RaiseAndSetIfChanged(ref _rT, value);
            get => _rT;
        }

        // Bindings

        [JsonProperty("TipActivationPressure")]
        public float TipActivationPressure
        {
            set => RaiseAndSetIfChanged(ref _tP, value);
            get => _tP;
        }

        [JsonProperty("TipButton")]
        public PluginSettingStore TipButton
        {
            set => RaiseAndSetIfChanged(ref _tipButton, value);
            get => _tipButton;
        }

        [JsonProperty("EraserActivationPressure")]
        public float EraserActivationPressure
        {
            set => this.RaiseAndSetIfChanged(ref _eP, value);
            get => _eP;
        }

        [JsonProperty("EraserButton")]
        public PluginSettingStore EraserButton
        {
            set => this.RaiseAndSetIfChanged(ref _eraserButton, value);
            get => _eraserButton;
        }

        [JsonProperty("PenButtons")]
        public PluginSettingStoreCollection PenButtons
        {
            set => RaiseAndSetIfChanged(ref _penButtons, value);
            get => _penButtons;
        }

        [JsonProperty("AuxButtons")]
        public PluginSettingStoreCollection AuxButtons
        {
            set => RaiseAndSetIfChanged(ref _auxButtons, value);
            get => _auxButtons;
        }

        // Tools

        public void SetDisplayArea(Area area)
        {
            _sizeChanging = true;
            DisplayWidth = area.Width;

            _sizeChanging = true;
            DisplayHeight = area.Height;

            DisplayX = area.Position.X;
            DisplayY = area.Position.Y;

            _sizeChanging = false;

            // Refresh aspect ratio lock
            if (LockAspectRatio)
            {
                LockAspectRatio = false;
                LockAspectRatio = true;
            }
        }

        public Area GetDisplayArea()
        {
            return new Area(DisplayWidth, DisplayHeight, new Vector2(DisplayX, DisplayY), 0);
        }

        public void SetTabletArea(Area area)
        {
            _sizeChanging = true;
            TabletWidth = area.Width;

            _sizeChanging = true;
            TabletHeight = area.Height;

            TabletX = area.Position.X;
            TabletY = area.Position.Y;
            TabletRotation = area.Rotation;

            _sizeChanging = false;

            // Refresh aspect ratio lock
            if (LockAspectRatio)
            {
                LockAspectRatio = false;
                LockAspectRatio = true;
            }
        }

        public Area GetTabletArea()
        {
            return new Area(TabletWidth, TabletHeight, new Vector2(TabletX, TabletY), TabletRotation);
        }

        public void Serialize()
        {
            ProfileSerializer.Serialize(this);
        }

        public override string ToString()
        {
            return ProfileName;
        }
    }
}