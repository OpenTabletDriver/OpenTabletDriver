using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using ReactiveUI;

namespace OpenTabletDriver.Models
{
    [XmlRoot("Configuration", DataType = "OpenTabletDriverCfg")]
    public class Settings : ReactiveObject
    {
        public Settings()
        {
            Theme = "Light";
            OutputMode = nameof(TabletDriverLib.Output.AbsoluteMode);
            WindowWidth = 1280;
            WindowHeight = 720;
            ResetTime = TimeSpan.FromMilliseconds(100);
            TipButton = "TabletDriverLib.Binding.MouseBinding, Left";
        }

        private float _dW, _dH, _dX, _dY, _dR, _tW, _tH, _tX, _tY, _tR, _xsens, _ysens;
        private bool _clipping, _autohook, _lockar, _sizeChanging;
        private string _theme, _outputMode, _activeFilter;
        private TimeSpan _resetTime;

        #region General Settings

        [XmlElement("Theme")]
        public string Theme
        {
            set
            {
                this.RaiseAndSetIfChanged(ref _theme, value);
                (App.Current as App).SetTheme(Themes.Parse(value));
            }
            get => _theme;
        }

        [XmlElement("OutputMode")]
        public string OutputMode
        {
            set => this.RaiseAndSetIfChanged(ref _outputMode, value != "{Disable}" ? value : null);
            get => _outputMode;
        }

        [XmlElement("ActiveFilter")]
        public string ActiveFilterName
        {
            set => this.RaiseAndSetIfChanged(ref _activeFilter, value != "{Disable}" ? value : null);
            get => _activeFilter;
        }

        [XmlElement("AutoHook")]
        public bool AutoHook
        {
            set => this.RaiseAndSetIfChanged(ref _autohook, value);
            get => _autohook;
        }
        
        #endregion

        #region Absolute Mode Settings

        [XmlElement("DisplayWidth")]
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

        [XmlElement("DisplayHeight")]
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

        [XmlElement("DisplayXOffset")]
        public float DisplayX
        {
            set => this.RaiseAndSetIfChanged(ref _dX, value);
            get => _dX;
        }

        [XmlElement("DisplayYOffset")]
        public float DisplayY
        {
            set => this.RaiseAndSetIfChanged(ref _dY, value);
            get => _dY;
        }

        [XmlElement("DisplayRotation")]
        public float DisplayRotation
        {
            set => this.RaiseAndSetIfChanged(ref _dR, value);
            get => _dR;
        }

        [XmlElement("TabletWidth")]
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

        [XmlElement("TabletHeight")]
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

        [XmlElement("TabletXOffset")]
        public float TabletX
        {
            set => this.RaiseAndSetIfChanged(ref _tX, value);
            get => _tX;
        }

        [XmlElement("TabletYOffset")]
        public float TabletY
        {
            set => this.RaiseAndSetIfChanged(ref _tY, value);
            get => _tY;
        }

        [XmlElement("TabletRotation")]
        public float TabletRotation
        {
            set => this.RaiseAndSetIfChanged(ref _tR, value);
            get => _tR;
        }

        [XmlElement("EnableClipping")]
        public bool EnableClipping
        {
            set => this.RaiseAndSetIfChanged(ref _clipping, value);
            get => _clipping;
        }

        [XmlElement("LockAspectRatio")]
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
        
        [XmlElement("XSensitivity")]
        public float XSensitivity
        {
            set => this.RaiseAndSetIfChanged(ref _xsens, value);
            get => _xsens;
        }

        [XmlElement("YSensitivity")]
        public float YSensitivity
        {
            set => this.RaiseAndSetIfChanged(ref _ysens, value);
            get => _ysens;
        }

        [XmlElement("RelativeResetDelay")]
        public TimeSpan ResetTime
        {
            set => this.RaiseAndSetIfChanged(ref _resetTime, value);
            get => _resetTime;
        }

        #endregion

        #region Bindings

        private float _tipPressure;

        [XmlElement("TipActivationPressure")]
        public float TipActivationPressure
        {
            set => this.RaiseAndSetIfChanged(ref _tipPressure, (float)Math.Round(value, 3));
            get => _tipPressure;
        }

        private string _tipButton;

        [XmlElement("TipButton")]
        public string TipButton
        {
            set => this.RaiseAndSetIfChanged(ref _tipButton, value);
            get => _tipButton;
        }

        private ObservableCollection<string> _penButtons;

        [XmlArray("PenButtons")]
        [XmlArrayItem("Binding")]
        public ObservableCollection<string> PenButtons
        {
            set => this.RaiseAndSetIfChanged(ref _penButtons, value);
            get => _penButtons;
        }

        private ObservableCollection<string> _auxButtons;

        [XmlArray("AuxButtons")]
        [XmlArrayItem("Binding")]
        public ObservableCollection<string> AuxButtons
        {
            set => this.RaiseAndSetIfChanged(ref _auxButtons, value);
            get => _auxButtons;
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

        #region XML Serialization

        private static readonly XmlSerializer XmlSerializer = new XmlSerializer(typeof(Settings));

        public static Settings Deserialize(FileInfo file)
        {
            using (var stream = file.OpenRead())
                return (Settings)XmlSerializer.Deserialize(stream);
        }

        public void Serialize(FileInfo file)
        {
            if (file.Exists)
                file.Delete();
            using (var stream = file.OpenWrite())
                XmlSerializer.Serialize(stream, this);
        }

        #endregion
    }
}