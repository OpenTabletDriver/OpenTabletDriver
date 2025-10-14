using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable enable

namespace OpenTabletDriver.Plugin.Tablet
{
    public class TabletSpecifications
    {
        /// <summary>
        /// Specifications for the tablet digitizer.
        /// </summary>
        [Required(ErrorMessage = $"{nameof(Digitizer)} specifications must be defined")]
        public DigitizerSpecifications Digitizer { set; get; } = new DigitizerSpecifications();

        /// <summary>
        /// Specifications for the tablet's pen.
        /// </summary>
        [Required(ErrorMessage = $"{nameof(Pen)} specifications must be defined")]
        public PenSpecifications Pen { set; get; } = new PenSpecifications();

        /// <summary>
        /// Specifications for the auxiliary buttons.
        /// </summary>
        public ButtonSpecifications? AuxiliaryButtons { set; get; }

        /// <summary>
        /// Specifications for the mouse buttons.
        /// </summary>
        public ButtonSpecifications? MouseButtons { set; get; }

        /// <summary>
        /// Specifications for the wheels.
        /// </summary>
        public List<WheelSpecifications>? Wheels { get; set; }

        /// <summary>
        /// Specifications for the strips.
        /// </summary>
        public List<AnalogSpecifications>? Strips { set; get; }

        /// <summary>
        /// Specifications for the touch digitizer.
        /// </summary>
        public DigitizerSpecifications? Touch { set; get; }
    }
}
