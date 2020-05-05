using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;

namespace TabletDriverLib
{
    public class Settings
    {
        public Settings()
        {
        }

        private float _dW, _dH, _tW, _tH;
        private bool _lockar, _sizeChanging;
        private string _outputMode;

        #region General Settings

        [JsonProperty("OutputMode")]
        public string OutputMode
        {
            set => _outputMode = value != "{Disable}" ? value : null;
            get => _outputMode;
        }

        [JsonProperty("Filters")]
        public ObservableCollection<string> Filters { set; get; }

        [JsonProperty("AutoHook")]
        public bool AutoHook { set; get; }

        #endregion

        #region Absolute Mode Settings

        [JsonProperty("DisplayWidth")]
        public float DisplayWidth
        {
            set
            {
                _dW = value;
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
                _dH = value;
                if (LockAspectRatio)
                    TabletWidth = DisplayWidth / DisplayHeight * TabletHeight;
            }
            get => _dH;
        }

        [JsonProperty("DisplayXOffset")]
        public float DisplayX { set; get; }

        [JsonProperty("DisplayYOffset")]
        public float DisplayY { set; get; }

        [JsonProperty("DisplayRotation")]
        public float DisplayRotation { set; get; }

        [JsonProperty("TabletWidth")]
        public float TabletWidth
        {
            set
            {
                _tW = value;
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
                _tH = value;
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
        public float TabletX { set; get; }

        [JsonProperty("TabletYOffset")]
        public float TabletY { set; get; }

        [JsonProperty("TabletRotation")]
        public float TabletRotation { set; get; }

        [JsonProperty("EnableClipping")]
        public bool EnableClipping { set; get; }

        [JsonProperty("LockAspectRatio")]
        public bool LockAspectRatio
        {
            set
            {
                _lockar = value;
                if (value)
                    TabletHeight = DisplayHeight / DisplayWidth * TabletWidth;
            }
            get => _lockar;
        }

        #endregion

        #region Relative Mode Settings

        [JsonProperty("XSensitivity")]
        public float XSensitivity { set; get; }

        [JsonProperty("YSensitivity")]
        public float YSensitivity { set; get; }

        [JsonProperty("RelativeResetDelay")]
        public TimeSpan ResetTime { set; get; }

        #endregion

        #region Bindings

        [JsonProperty("TipActivationPressure")]
        public float TipActivationPressure { set; get; }

        [JsonProperty("TipButton")]
        public string TipButton { set; get; }

        [JsonProperty("PenButtons")]
        public ObservableCollection<string> PenButtons { set; get; }

        [JsonProperty("AuxButtons")]
        public ObservableCollection<string> AuxButtons { set; get; }

        [JsonProperty("PluginSettings")]
        public Dictionary<string, string> PluginSettings { set; get; }

        [JsonProperty("ResidentPlugins")]
        public ObservableCollection<string> ResidentPlugins { set; get; }

        #endregion

        #region Json Serialization

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

        #region Defaults

        public static readonly Settings Defaults = new Settings
        {
            OutputMode = typeof(TabletDriverLib.Output.AbsoluteMode).FullName,
            AutoHook = true,
            EnableClipping = true,
            TipButton = "TabletDriverLib.Binding.MouseBinding, Left",
            TipActivationPressure = 1,
            PenButtons = new ObservableCollection<string>(new string[2]),
            AuxButtons = new ObservableCollection<string>(new string[4]),
            PluginSettings = new Dictionary<string, string>(),
            XSensitivity = 10,
            YSensitivity = 10,
            ResetTime = TimeSpan.FromMilliseconds(100)
        };

        #endregion
    }
}