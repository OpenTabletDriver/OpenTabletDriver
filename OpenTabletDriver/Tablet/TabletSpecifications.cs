using System.ComponentModel;
using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// Device specifications for a tablet device.
    /// </summary>
    [PublicAPI]
    public class TabletSpecifications
    {
        /// <summary>
        /// Specifications for the tablet digitizer.
        /// </summary>
        public DigitizerSpecifications? Digitizer { set; get; }

        /// <summary>
        /// Specifications for the tablet's pen.
        /// </summary>
        public PenSpecifications? Pen { set; get; }

        /// <summary>
        /// Specifications for the auxiliary buttons.
        /// </summary>
        [DisplayName("Auxiliary")]
        public ButtonSpecifications? AuxiliaryButtons { set; get; }

        /// <summary>
        /// Specifications for the mouse buttons.
        /// </summary>
        [DisplayName("Mouse")]
        public ButtonSpecifications? MouseButtons { set; get; }

        /// <summary>
        /// Specifications for the touch digitizer.
        /// </summary>
        public DigitizerSpecifications? Touch { set; get; }

        /// <summary>
        /// Specifications for the touchpad gestures.
        /// </summary>
        public GestureTouchpadSpecifications? GestureTouchpad { set; get; }
    }
}
