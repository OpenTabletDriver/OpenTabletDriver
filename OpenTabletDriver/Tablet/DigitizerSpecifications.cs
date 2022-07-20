using System.ComponentModel;
using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// Specifications for a digitizer.
    /// </summary>
    [PublicAPI]
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
        [DisplayName("Max X")]
        public float MaxX { set; get; }

        /// <summary>
        /// The maximum Y coordinate for the digitizer.
        /// </summary>
        [DisplayName("Max Y")]
        public float MaxY { set; get; }
    }
}
