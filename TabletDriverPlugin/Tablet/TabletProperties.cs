using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace TabletDriverPlugin.Tablet
{
    [XmlRoot(Namespace = "TabletDriverLib", ElementName = "Tablet")]
    public class TabletProperties
    {
        /// <summary>
        /// The device's name.
        /// </summary>
        /// <value></value>
        [XmlElement("Name")]
        public string TabletName { set; get; } = string.Empty;

        /// <summary>
        /// The device's vendor ID.
        /// </summary>
        /// <value></value>
        [XmlElement("VendorID")]
        public int VendorID { set; get; } = 0;

        /// <summary>
        /// The device's product ID.
        /// </summary>
        /// <value></value>
        [XmlElement("ProductID")]
        public int ProductID { set; get; } = 0;

        /// <summary>
        /// The device's report length.
        /// </summary>
        /// <value></value>
        [XmlElement("InputReportLength")]
        public uint InputReportLength { set; get; } = 0;

        /// <summary>
        /// The device's report parser type.
        /// </summary>
        /// <value></value>
        [XmlElement("ReportParser")]
        public string ReportParserName { set; get; }

        /// <summary>
        /// The device's input report length when a custom report parser is needed.
        /// </summary>
        /// <value></value>
        [XmlElement("CustomInputReportLength")]
        public uint CustomInputReportLength { set; get; } = 0;

        /// <summary>
        /// The type path of the custom report parser to be used.
        /// </summary>
        /// <value></value>
        [XmlElement("CustomReportParser")]
        public string CustomReportParserName { set; get; }

        /// <summary>
        /// The device's horizontal active area in millimeters.
        /// </summary>
        /// <value></value>
        [XmlElement("Width")]
        public float Width { set; get; } = 0;

        /// <summary>
        /// The device's vertical active area in millimeters.
        /// </summary>
        /// <value></value>
        [XmlElement("Height")]
        public float Height { set; get; } = 0;

        /// <summary>
        /// The device's maximum horizontal input.
        /// </summary>
        /// <value></value>
        [XmlElement("MaxX")]
        public float MaxX { set; get; } = 0;

        /// <summary>
        /// The device's maximum vertical input.
        /// </summary>
        /// <value></value>
        [XmlElement("MaxY")]
        public float MaxY { set; get; } = 0;

        /// <summary>
        /// The device's maximum input pressure detection value.
        /// </summary>
        /// <value></value>
        [XmlElement("MaxPressure")]
        public uint MaxPressure { set; get; } = 0;

        /// <summary>
        /// The device's minimum detection lift.
        /// </summary>
        /// <value></value>
        [XmlElement("MinimumRange")]
        public uint MinimumRange { set; get; } = 0;

        /// <summary>
        /// The report length of the device's auxiliary hid device, if it has one.
        /// </summary>
        /// <value></value>
        [XmlElement("AuxReportLength")]
        public uint AuxReportLength { set; get; } = 0;

        /// <summary>
        /// The report parser used by the auxiliary hid device.
        /// </summary>
        /// <value></value>
        [XmlElement("AuxReportParser")]
        public string AuxReportParserName { set; get; }

        /// <summary>
        /// The feature report sent to initialize the tablet's functions.
        /// </summary>
        /// <value></value>
        [XmlElement("FeatureInitReport")]
        public byte[] FeatureInitReport { set; get; }

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