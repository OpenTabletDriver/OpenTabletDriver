namespace TabletDriverLib.Class
{
    public class TabletProperties
    {
        /// <summary>
        /// The device's horizontal active area in millimeters
        /// </summary>
        /// <value></value>
        public float Width { private set; get; } = 0;

        /// <summary>
        /// The device's vertical active area in millimeters 
        /// </summary>
        /// <value></value>
        public float Height { private set; get; } = 0;

        /// <summary>
        /// The device's maximum horizontal input
        /// </summary>
        /// <value></value>
        public float MaxX { private set; get; } = 0;

        /// <summary>
        /// The device's maximum vertical input
        /// </summary>
        /// <value></value>
        public float MaxY { private set; get; } = 0;

        /// <summary>
        /// The device's maximum input pressure detection value
        /// </summary>
        /// <value></value>
        public int MaxPressure { private set; get; } = 0;
    }
}