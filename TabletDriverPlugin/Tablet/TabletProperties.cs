using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace TabletDriverPlugin.Tablet
{
    public class TabletProperties
    {
        /// <summary>
        /// The tablet's name.
        /// </summary>
        [JsonProperty("Name")]
        public string Name { set; get; }

        /// <summary>
        /// The digitizer device identifier.
        /// </summary>
        public DeviceIdentifier DigitizerIdentifier { set; get; } = new DeviceIdentifier();

        /// <summary>
        /// The alternate digitizer device identifier.
        /// </summary>
        public DeviceIdentifier AlternateDigitizerIdentifier { set; get; } = new DeviceIdentifier();

        /// <summary>
        /// The auxiliary device identifier.
        /// </summary>
        public DeviceIdentifier AuxilaryDeviceIdentifier { set; get; } = new DeviceIdentifier();

        /// <summary>
        /// The tablet's horizontal active area in millimeters.
        /// </summary>
        [JsonProperty("Width")]
        public float Width { set; get; }

        /// <summary>
        /// The tablet's vertical active area in millimeters.
        /// </summary>
        [JsonProperty("Height")]
        public float Height { set; get; }

        /// <summary>
        /// The tablet's maximum horizontal input.
        /// </summary>
        [JsonProperty("MaxX")]
        public float MaxX { set; get; }

        /// <summary>
        /// The tablet's maximum vertical input.
        /// </summary>
        [JsonProperty("MaxY")]
        public float MaxY { set; get; }

        /// <summary>
        /// The tablet's maximum input pressure detection value.
        /// </summary>
        [JsonProperty("MaxPressure")]
        public uint MaxPressure { set; get; }

        /// <summary>
        /// The tablet's minimum detection report ID.
        /// </summary>
        [JsonProperty("ActiveReportID")]
        public uint ActiveReportID { set; get; }

        /// <summary>
        /// Other information about the tablet that can be used in tools or other applications.
        /// </summary>
        public Dictionary<string, string> Attributes { set; get; } = new Dictionary<string, string>();

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