using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using ReactiveUI;

namespace OpenTabletDriverGUI.Models
{
    [XmlRoot("Configuration", DataType = "OpenTabletDriverCfg")]
    public class Settings : ReactiveObject
    {
        public Settings()
        {
            Theme = "Light";
        }

        #region Properties

        private float _dW, _dH, _dX, _dY, _dR, _tW, _tH, _tX, _tY, _tR;
        private bool _clipping;
        private string _theme;

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

        [XmlElement("EnableClipping")]
        public bool EnableClipping
        {
            set => this.RaiseAndSetIfChanged(ref _clipping, value);
            get => _clipping;
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