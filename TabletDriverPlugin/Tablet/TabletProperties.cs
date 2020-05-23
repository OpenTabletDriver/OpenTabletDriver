using System.IO;
using Newtonsoft.Json;

namespace TabletDriverPlugin.Tablet
{
    public class TabletProperties : Notifier
    {
        private string _tabletName, _reportParser, _customReportParser, _auxReportParser;
        private int _vid, _pid;
        private uint _inputReportLength, _customInputReportLength, _reportId, _auxReportLength, _outputReportLength, _maxPressure;
        private float _width, _height, _maxX, _maxY;
        private byte[] _featureInitReport, _outputInitReport;

        /// <summary>
        /// The device's name.
        /// </summary>
        /// <value></value>
        [JsonProperty("Name")]
        public string TabletName
        {
            set => this.RaiseAndSetIfChanged(ref _tabletName, value);
            get => _tabletName;
        }

        /// <summary>
        /// The device's vendor ID.
        /// </summary>
        /// <value></value>
        [JsonProperty("VendorID")]
        public int VendorID
        {
            set => this.RaiseAndSetIfChanged(ref _vid, value);
            get => _vid;
        }

        /// <summary>
        /// The device's product ID.
        /// </summary>
        /// <value></value>
        [JsonProperty("ProductID")]
        public int ProductID
        {
            set => this.RaiseAndSetIfChanged(ref _pid, value);
            get => _pid;
        }

        /// <summary>
        /// The device's report length.
        /// </summary>
        /// <value></value>
        [JsonProperty("InputReportLength")]
        public uint InputReportLength
        {
            set => this.RaiseAndSetIfChanged(ref _inputReportLength, value);
            get => _inputReportLength;
        }

        /// <summary>
        /// The device's output report length.
        /// If the value is 0, it isn't used as an identifier.
        /// </summary>
        /// <value></value>
        [JsonProperty("OutputReportLength")]
        public uint OutputReportLength
        {
            set => this.RaiseAndSetIfChanged(ref _outputReportLength, value);
            get => _outputReportLength;
        }

        /// <summary>
        /// The device's report parser type.
        /// </summary>
        /// <value></value>
        [JsonProperty("ReportParser")]
        public string ReportParserName
        {
            set => this.RaiseAndSetIfChanged(ref _reportParser, value);
            get => _reportParser;
        }

        /// <summary>
        /// The device's input report length when a custom report parser is needed.
        /// </summary>
        /// <value></value>
        [JsonProperty("CustomInputReportLength")]
        public uint CustomInputReportLength
        {
            set => this.RaiseAndSetIfChanged(ref _customInputReportLength, value);
            get => _customInputReportLength;
        }

        /// <summary>
        /// The type path of the custom report parser to be used.
        /// </summary>
        /// <value></value>
        [JsonProperty("CustomReportParser")]
        public string CustomReportParserName
        {
            set => this.RaiseAndSetIfChanged(ref _customReportParser, value);
            get => _customReportParser;
        }

        /// <summary>
        /// The device's horizontal active area in millimeters.
        /// </summary>
        /// <value></value>
        [JsonProperty("Width")]
        public float Width
        {
            set => this.RaiseAndSetIfChanged(ref _width, value);
            get => _width;
        }

        /// <summary>
        /// The device's vertical active area in millimeters.
        /// </summary>
        /// <value></value>
        [JsonProperty("Height")]
        public float Height
        {
            set => this.RaiseAndSetIfChanged(ref _height, value);
            get => _height;
        }

        /// <summary>
        /// The device's maximum horizontal input.
        /// </summary>
        /// <value></value>
        [JsonProperty("MaxX")]
        public float MaxX
        {
            set => this.RaiseAndSetIfChanged(ref _maxX, value);
            get => _maxX;
        }

        /// <summary>
        /// The device's maximum vertical input.
        /// </summary>
        /// <value></value>
        [JsonProperty("MaxY")]
        public float MaxY
        {
            set => this.RaiseAndSetIfChanged(ref _maxY, value);
            get => _maxY;
        }

        /// <summary>
        /// The device's maximum input pressure detection value.
        /// </summary>
        /// <value></value>
        [JsonProperty("MaxPressure")]
        public uint MaxPressure
        {
            set => this.RaiseAndSetIfChanged(ref _maxPressure, value);
            get => _maxPressure;
        }

        /// <summary>
        /// The device's minimum detection report ID.
        /// </summary>
        /// <value></value>
        [JsonProperty("ActiveReportID")]
        public uint ActiveReportID
        {
            set => this.RaiseAndSetIfChanged(ref _reportId, value);
            get => _reportId;
        }

        /// <summary>
        /// The report length of the device's auxiliary hid device, if it has one.
        /// </summary>
        /// <value></value>
        [JsonProperty("AuxReportLength")]
        public uint AuxReportLength
        {
            set => this.RaiseAndSetIfChanged(ref _auxReportLength, value);
            get => _auxReportLength;
        }

        /// <summary>
        /// The report parser used by the auxiliary hid device.
        /// </summary>
        /// <value></value>
        [JsonProperty("AuxReportParser")]
        public string AuxReportParserName
        {
            set => this.RaiseAndSetIfChanged(ref _auxReportParser, value);
            get => _auxReportParser;
        }

        /// <summary>
        /// The feature report sent to initialize the tablet's functions.
        /// </summary>
        /// <value></value>
        [JsonProperty("FeatureInitReport")]
        public byte[] FeatureInitReport
        {
            set => this.RaiseAndSetIfChanged(ref _featureInitReport, value);
            get => _featureInitReport;
        }

        /// <summary>
        /// The output report sent to initialize the tablet's functions.
        /// </summary>
        /// <value></value>
        [JsonProperty("OutputInitReport")]
        public byte[] OutputInitReport
        {
            set => this.RaiseAndSetIfChanged(ref _outputInitReport, value);
            get => _outputInitReport;
        }

        #region Json Serialization
            
        public void Write(FileInfo file)
        {
            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(file.FullName, str);
        }

        public static TabletProperties Read(FileInfo file)
        {
            using (var stream = file.OpenRead())
            using (var sr = new StreamReader(stream))
            {
                var str = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<TabletProperties>(str);
            }
        }

        #endregion
    }
}