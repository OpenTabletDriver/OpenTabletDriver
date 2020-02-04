using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace TabletDriverPlugin.Tablet
{
    [XmlRoot(Namespace = "TabletDriverLib", ElementName = "Tablet")]
    public class TabletProperties : Notifier
    {
        private string _tabletName, _reportParser, _customReportParser, _auxReportParser;
        private int _vid, _pid;
        private uint _inputReportLength, _customInputReportLength, _minimumRange, _auxReportLength, _maxPressure;
        private float _width, _height, _maxX, _maxY;
        private byte[] _featureInitReport;

        /// <summary>
        /// The device's name.
        /// </summary>
        /// <value></value>
        [XmlElement("Name")]
        public string TabletName
        {
            set => this.RaiseAndSetIfChanged(ref _tabletName, value);
            get => _tabletName;
        }

        /// <summary>
        /// The device's vendor ID.
        /// </summary>
        /// <value></value>
        [XmlElement("VendorID")]
        public int VendorID
        {
            set => this.RaiseAndSetIfChanged(ref _vid, value);
            get => _vid;
        }

        /// <summary>
        /// The device's product ID.
        /// </summary>
        /// <value></value>
        [XmlElement("ProductID")]
        public int ProductID
        {
            set => this.RaiseAndSetIfChanged(ref _pid, value);
            get => _pid;
        }

        /// <summary>
        /// The device's report length.
        /// </summary>
        /// <value></value>
        [XmlElement("InputReportLength")]
        public uint InputReportLength
        {
            set => this.RaiseAndSetIfChanged(ref _inputReportLength, value);
            get => _inputReportLength;
        }

        /// <summary>
        /// The device's report parser type.
        /// </summary>
        /// <value></value>
        [XmlElement("ReportParser")]
        public string ReportParserName
        {
            set => this.RaiseAndSetIfChanged(ref _reportParser, value);
            get => _reportParser;
        }

        /// <summary>
        /// The device's input report length when a custom report parser is needed.
        /// </summary>
        /// <value></value>
        [XmlElement("CustomInputReportLength")]
        public uint CustomInputReportLength
        {
            set => this.RaiseAndSetIfChanged(ref _customInputReportLength, value);
            get => _customInputReportLength;
        }

        /// <summary>
        /// The type path of the custom report parser to be used.
        /// </summary>
        /// <value></value>
        [XmlElement("CustomReportParser")]
        public string CustomReportParserName
        {
            set => this.RaiseAndSetIfChanged(ref _customReportParser, value);
            get => _customReportParser;
        }

        /// <summary>
        /// The device's horizontal active area in millimeters.
        /// </summary>
        /// <value></value>
        [XmlElement("Width")]
        public float Width
        {
            set => this.RaiseAndSetIfChanged(ref _width, value);
            get => _width;
        }

        /// <summary>
        /// The device's vertical active area in millimeters.
        /// </summary>
        /// <value></value>
        [XmlElement("Height")]
        public float Height
        {
            set => this.RaiseAndSetIfChanged(ref _height, value);
            get => _height;
        }

        /// <summary>
        /// The device's maximum horizontal input.
        /// </summary>
        /// <value></value>
        [XmlElement("MaxX")]
        public float MaxX
        {
            set => this.RaiseAndSetIfChanged(ref _maxX, value);
            get => _maxX;
        }

        /// <summary>
        /// The device's maximum vertical input.
        /// </summary>
        /// <value></value>
        [XmlElement("MaxY")]
        public float MaxY
        {
            set => this.RaiseAndSetIfChanged(ref _maxY, value);
            get => _maxY;
        }

        /// <summary>
        /// The device's maximum input pressure detection value.
        /// </summary>
        /// <value></value>
        [XmlElement("MaxPressure")]
        public uint MaxPressure
        {
            set => this.RaiseAndSetIfChanged(ref _maxPressure, value);
            get => _maxPressure;
        }

        /// <summary>
        /// The device's minimum detection lift.
        /// </summary>
        /// <value></value>
        [XmlElement("MinimumRange")]
        public uint MinimumRange
        {
            set => this.RaiseAndSetIfChanged(ref _minimumRange, value);
            get => _minimumRange;
        }

        /// <summary>
        /// The report length of the device's auxiliary hid device, if it has one.
        /// </summary>
        /// <value></value>
        [XmlElement("AuxReportLength")]
        public uint AuxReportLength
        {
            set => this.RaiseAndSetIfChanged(ref _auxReportLength, value);
            get => _auxReportLength;
        }

        /// <summary>
        /// The report parser used by the auxiliary hid device.
        /// </summary>
        /// <value></value>
        [XmlElement("AuxReportParser")]
        public string AuxReportParserName
        {
            set => this.RaiseAndSetIfChanged(ref _auxReportParser, value);
            get => _auxReportParser;
        }

        /// <summary>
        /// The feature report sent to initialize the tablet's functions.
        /// </summary>
        /// <value></value>
        [XmlElement("FeatureInitReport")]
        public byte[] FeatureInitReport
        {
            set => this.RaiseAndSetIfChanged(ref _featureInitReport, value);
            get => _featureInitReport;
        }

        #region XML Serialization

        private static readonly XmlSerializer XmlSerializer = new XmlSerializer(typeof(TabletProperties));

        public void Write(FileInfo file)
        {
            if (file.Exists)
                file.Delete();

            if (!file.Directory.Exists)
                file.Directory.Create();

            using (var fs = file.OpenWrite())
                XmlSerializer.Serialize(fs, this);
        }

        public static TabletProperties Read(FileInfo file)
        {
            using (var fs = file.OpenRead())
                return (TabletProperties)XmlSerializer.Deserialize(fs);
        }

        #endregion
    }
}