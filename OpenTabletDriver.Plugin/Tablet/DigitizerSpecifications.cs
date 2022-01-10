namespace OpenTabletDriver.Plugin.Tablet
{
    public class DigitizerSpecifications
    {
        /// <summary>
        /// The width of the digitizer in millimeters.
        /// </summary>
        public float Width { set; get; }

        /// <summary>
        /// The height of the digitizer in millimeters.
        /// </summary>
        public float Height { set; get; }

        /// <summary>
        /// The maximum X coordinate for the digitizer.
        /// </summary>
        public float MaxX { set; get; }

        /// <summary>
        /// The maximum Y coordinate for the digitizer.
        /// </summary>
        public float MaxY { set; get; }
    }
}
