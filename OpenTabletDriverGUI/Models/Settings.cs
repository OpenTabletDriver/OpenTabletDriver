using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ReactiveUI;
using TabletDriverLib.Interop.Cursor;

namespace OpenTabletDriverGUI.Models
{
    [XmlRoot("Configuration", DataType = "OpenTabletDriverCfg")]
    public class Settings : ReactiveObject
    {
        public Settings()
        {
            Theme = "Light";
            OutputMode = "Absolute";
        }

        #region Properties

        private float _dW, _dH, _dX, _dY, _dR, _tW, _tH, _tX, _tY, _tR;
        private bool _clipping, _autohook;
        private string _theme, _outputMode;

        [XmlElement("DisplayWidth")]
        public float DisplayWidth 
        {
            set => this.RaiseAndSetIfChanged(ref _dW, value);
            get => _dW;
        }

        [XmlElement("DisplayHeight")]
        public float DisplayHeight
        {
            set => this.RaiseAndSetIfChanged(ref _dH, value);
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
            set => this.RaiseAndSetIfChanged(ref _tW, value);
            get => _tW;
        }

        [XmlElement("TabletHeight")]
        public float TabletHeight
        {
            set => this.RaiseAndSetIfChanged(ref _tH, value);
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
            set => this.RaiseAndSetIfChanged(ref _outputMode, value);
            get => _outputMode;
        }

        [XmlElement("EnableClipping")]
        public bool EnableClipping
        {
            set => this.RaiseAndSetIfChanged(ref _clipping, value);
            get => _clipping;
        }

        [XmlElement("AutoHook")]
        public bool AutoHook
        {
            set => this.RaiseAndSetIfChanged(ref _autohook, value);
            get => _autohook;
        }
        
        #endregion

        #region Button Bindings

        private float _tipPressure;

        [XmlElement("TipActivationPressure")]
        public float TipActivationPressure
        {
            set => this.RaiseAndSetIfChanged(ref _tipPressure, (float)Math.Round(value, 3));
            get => _tipPressure;
        }

        private MouseButton _tipButton;

        [XmlElement("TipButton")]
        public MouseButton TipButton
        {
            set => this.RaiseAndSetIfChanged(ref _tipButton, value);
            get => _tipButton;
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
            using (var stream = file.OpenWrite())
                XmlSerializer.Serialize(stream, this);
        }

        #endregion
    }
}