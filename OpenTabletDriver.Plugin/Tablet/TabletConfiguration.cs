using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace OpenTabletDriver.Plugin.Tablet
{
    public class TabletConfiguration
    {
        /// <summary>
        /// The tablet's name.
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        /// The digitizer device identifier.
        /// </summary>
        public List<DigitizerIdentifier> DigitizerIdentifiers { set; get; } = new List<DigitizerIdentifier>();

        /// <summary>
        /// The auxiliary device identifier.
        /// </summary>
        public List<DeviceIdentifier> AuxilaryDeviceIdentifiers { set; get; } = new List<DeviceIdentifier>();

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

        public static TabletConfiguration Read(FileInfo file)
        {
            using (var stream = file.OpenRead())
            using (var sr = new StreamReader(stream))
            {
                var str = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<TabletConfiguration>(str);
            }
        }

        #endregion
    }
}