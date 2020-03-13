using System;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;
using ReactiveUI;

namespace OpenTabletDriver.Models
{
    public class Settings : ReactiveObject
    {
        public Settings()
        {
        }

        private float _dW, _dH, _dX, _dY, _dR, _tW, _tH, _tX, _tY, _tR, _xsens, _ysens;
        private bool _clipping, _autohook, _lockar, _sizeChanging;
        private string _theme, _outputMode;
        private TimeSpan _resetTime;
        private ObservableCollection<string> _filters, _residents = new ObservableCollection<string>();

        #region General Settings

        [JsonProperty("Theme")]
        public string Theme
        {
            set => this.RaiseAndSetIfChanged(ref _theme, value);
            get => _theme;
        }

        [JsonProperty("OutputMode")]
        public string OutputMode
        {
            set => this.RaiseAndSetIfChanged(ref _outputMode, value != "{Disable}" ? value : null);
            get => _outputMode;
        }

        [JsonProperty("Filters")]
        public ObservableCollection<string> Filters
        {
            set => this.RaiseAndSetIfChanged(ref _filters, value);
            get => _filters;
        }

        [JsonProperty("AutoHook")]
        public bool AutoHook
        {
            set => this.RaiseAndSetIfChanged(ref _autohook, value);
            get => _autohook;
        }
        
        #endregion

        #region Absolute Mode Settings

        [JsonProperty("DisplayWidth")]
        public float DisplayWidth 
        {
            set
            {
                this.RaiseAndSetIfChanged(ref _dW, value);
                if (LockAspectRatio)
                    TabletHeight = (DisplayHeight / DisplayWidth) * TabletWidth;
            }
            get => _dW;
        }

        [JsonProperty("DisplayHeight")]
        public float DisplayHeight
        {
            set
            {
                this.RaiseAndSetIfChanged(ref _dH, value);
                if (LockAspectRatio)
                    TabletWidth = (DisplayWidth / DisplayHeight) * TabletHeight;
            }
            get => _dH;
        }

        [JsonProperty("DisplayXOffset")]
        public float DisplayX
        {
            set => this.RaiseAndSetIfChanged(ref _dX, value);
            get => _dX;
        }

        [JsonProperty("DisplayYOffset")]
        public float DisplayY
        {
            set => this.RaiseAndSetIfChanged(ref _dY, value);
            get => _dY;
        }

        [JsonProperty("DisplayRotation")]
        public float DisplayRotation
        {
            set => this.RaiseAndSetIfChanged(ref _dR, value);
            get => _dR;
        }

        [JsonProperty("TabletWidth")]
        public float TabletWidth
        {
            set
            {
                this.RaiseAndSetIfChanged(ref _tW, value);
                if (LockAspectRatio && !_sizeChanging)
                {
                    _sizeChanging = true;
                    TabletHeight = (DisplayHeight / DisplayWidth) * value;
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
                this.RaiseAndSetIfChanged(ref _tH, value);
                if (LockAspectRatio && !_sizeChanging)
                {
                    _sizeChanging = true;
                    TabletWidth = (DisplayWidth / DisplayHeight) * value; 
                    _sizeChanging = false;
                }
            }
            get => _tH;
        }

        [JsonProperty("TabletXOffset")]
        public float TabletX
        {
            set => this.RaiseAndSetIfChanged(ref _tX, value);
            get => _tX;
        }

        [JsonProperty("TabletYOffset")]
        public float TabletY
        {
            set => this.RaiseAndSetIfChanged(ref _tY, value);
            get => _tY;
        }

        [JsonProperty("TabletRotation")]
        public float TabletRotation
        {
            set => this.RaiseAndSetIfChanged(ref _tR, value);
            get => _tR;
        }

        [JsonProperty("EnableClipping")]
        public bool EnableClipping
        {
            set => this.RaiseAndSetIfChanged(ref _clipping, value);
            get => _clipping;
        }

        [JsonProperty("LockAspectRatio")]
        public bool LockAspectRatio
        {
            set
            {
                this.RaiseAndSetIfChanged(ref _lockar, value);
                if (value)
                    TabletHeight = (DisplayHeight / DisplayWidth) * TabletWidth;
            }
            get => _lockar;
        }

        #endregion

        #region Relative Mode Settings
        
        [JsonProperty("XSensitivity")]
        public float XSensitivity
        {
            set => this.RaiseAndSetIfChanged(ref _xsens, value);
            get => _xsens;
        }

        [JsonProperty("YSensitivity")]
        public float YSensitivity
        {
            set => this.RaiseAndSetIfChanged(ref _ysens, value);
            get => _ysens;
        }

        [JsonProperty("RelativeResetDelay")]
        public TimeSpan ResetTime
        {
            set => this.RaiseAndSetIfChanged(ref _resetTime, value);
            get => _resetTime;
        }

        #endregion

        #region Bindings

        private float _tipPressure;

        [JsonProperty("TipActivationPressure")]
        public float TipActivationPressure
        {
            set => this.RaiseAndSetIfChanged(ref _tipPressure, (float)Math.Round(value, 3));
            get => _tipPressure;
        }

        private string _tipButton;

        [JsonProperty("TipButton")]
        public string TipButton
        {
            set => this.RaiseAndSetIfChanged(ref _tipButton, value);
            get => _tipButton;
        }

        private ObservableCollection<string> _penButtons;

        [JsonProperty("PenButtons")]
        public ObservableCollection<string> PenButtons
        {
            set => this.RaiseAndSetIfChanged(ref _penButtons, value);
            get => _penButtons;
        }

        private ObservableCollection<string> _auxButtons;

        [JsonProperty("AuxButtons")]
        public ObservableCollection<string> AuxButtons
        {
            set => this.RaiseAndSetIfChanged(ref _auxButtons, value);
            get => _auxButtons;
        }

        private SerializableDictionary<string, string> _pluginSettings = new SerializableDictionary<string, string>();
        [JsonProperty("PluginSettings")]
        public SerializableDictionary<string, string> PluginSettings
        {
            set => this.RaiseAndSetIfChanged(ref _pluginSettings, value);
            get => _pluginSettings;
        }

        [JsonProperty("ResidentPlugins")]
        public ObservableCollection<string> ResidentPlugins
        {
            set => this.RaiseAndSetIfChanged(ref _residents, value);
            get => _residents;
        }

        #endregion

        #region Window Properties
        
        private int _windowWidth, _windowHeight;

        public int WindowWidth
        {
            set => this.RaiseAndSetIfChanged(ref _windowWidth, value);
            get => _windowWidth;
        }

        public int WindowHeight
        {
            set => this.RaiseAndSetIfChanged(ref _windowHeight, value);
            get => _windowHeight;
        }
        
        #endregion

        #region JSON Serialization
            
        public static Settings Deserialize(FileInfo file)
        {
            using (var stream = file.OpenRead())
            using (var sr = new StreamReader(stream))
            {
                var str = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<Settings>(str);
            }
        }

        public void Serialize(FileInfo file)
        {
            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(file.FullName, str);
        }

        #endregion
    }
}