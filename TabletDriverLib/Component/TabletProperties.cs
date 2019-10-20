using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace TabletDriverLib.Component
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
        /// The device number. 
        /// Tablets can have multiple devices associated with it, depending on your platform.
        /// </summary>
        /// <value></value>
        [XmlIgnore]
        public int DeviceNumber 
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                    case PlatformID.WinCE:
                        return WindowsDeviceNumber;
                    case PlatformID.Unix:
                        return LinuxDeviceNumber;
                    case PlatformID.MacOSX:
                        return MacOSXDeviceNumber;
                    default:
                        throw new NotImplementedException();
                }
            }
            set
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                    case PlatformID.WinCE:
                        WindowsDeviceNumber = value;
                        return;
                    case PlatformID.Unix:
                        LinuxDeviceNumber = value;
                        return;
                    case PlatformID.MacOSX:
                        MacOSXDeviceNumber = value;
                        return;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// The device number on the Linux platform.
        /// </summary>
        /// <value></value>
        [XmlElement("LinuxDeviceNumber")]
        public int LinuxDeviceNumber { set; get; } = 0; 

        /// <summary>
        /// The device number on the Windows platform.
        /// </summary>
        /// <value></value>
        [XmlElement("WindowsDeviceNumber")]
        public int WindowsDeviceNumber { set; get; } = 0;

        /// <summary>
        /// The device number on the Mac OS X platform.
        /// </summary>
        /// <value></value>
        [XmlElement("MacOSXDeviceNumber")]
        public int MacOSXDeviceNumber { set; get; } = 0;

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

        #region Math

        public float ProportionX => MaxX / Width;
        public float ProportionY => MaxY / Width;

        #endregion

        #region XML Serialization

        private static readonly XmlSerializer XmlSerializer = new XmlSerializer(typeof(TabletProperties));

        public void Write(FileInfo file)
        {
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