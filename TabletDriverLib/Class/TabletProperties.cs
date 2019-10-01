using System;

namespace TabletDriverLib.Class
{
    public class TabletProperties
    {
        public TabletProperties()
        {
        }

        public TabletProperties(Guid gUID, string tabletName, float width, float height, float maxX, float maxY, uint maxPressure)
        {
            GUID = gUID;
            TabletName = tabletName;
            Width = width;
            Height = height;
            MaxX = maxX;
            MaxY = maxY;
            MaxPressure = maxPressure;
        }

        /// <summary>
        /// The device's GUID
        /// </summary>
        /// <value></value>
        public Guid GUID { set; get; } = Guid.Empty;
        
        /// <summary>
        /// The device's name
        /// </summary>
        /// <value></value>
        public string TabletName { set; get; } = string.Empty;

        /// <summary>
        /// The device's horizontal active area in millimeters
        /// </summary>
        /// <value></value>
        public float Width { set; get; } = 0;

        /// <summary>
        /// The device's vertical active area in millimeters 
        /// </summary>
        /// <value></value>
        public float Height { set; get; } = 0;

        /// <summary>
        /// The device's maximum horizontal input
        /// </summary>
        /// <value></value>
        public float MaxX { set; get; } = 0;

        /// <summary>
        /// The device's maximum vertical input
        /// </summary>
        /// <value></value>
        public float MaxY { set; get; } = 0;

        /// <summary>
        /// The device's maximum input pressure detection value
        /// </summary>
        /// <value></value>
        public uint MaxPressure { set; get; } = 0;
    }
}