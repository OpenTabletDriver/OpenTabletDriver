namespace OpenTabletDriver.Plugin.Tablet
{
    public class DigitizerIdentifier : DeviceIdentifier
    {
        /// <summary>
        /// The tablet's horizontal active area in millimeters.
        /// </summary>
        public float Width { set; get; }

        /// <summary>
        /// The tablet's vertical active area in millimeters.
        /// </summary>
        public float Height { set; get; }

        /// <summary>
        /// The tablet's maximum horizontal input.
        /// </summary>
        public float MaxX { set; get; }

        /// <summary>
        /// The tablet's maximum vertical input.
        /// </summary>
        public float MaxY { set; get; }

        /// <summary>
        /// The tablet's maximum input pressure detection value.
        /// </summary>
        public uint MaxPressure { set; get; }

        /// <summary>
        /// The tablet's maximum supported pen button count, excluding eraser.
        /// </summary>
        public uint MaxPenButtonCount { set; get; }

        /// <summary>
        /// The tablet's active detection report ID.
        /// </summary>
        public DetectionRange ActiveReportID { set; get; }
    }
}