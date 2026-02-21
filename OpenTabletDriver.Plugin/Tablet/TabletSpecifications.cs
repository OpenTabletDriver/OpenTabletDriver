using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OpenTabletDriver.Plugin.Tablet
{
    public class TabletSpecifications
    {
        /// <summary>
        /// Specifications for the tablet digitizer.
        /// </summary>
        [JsonRequired, JsonProperty(nameof(Digitizer))]
        [Required(ErrorMessage = $"{nameof(Digitizer)} specifications must be defined")]
        public required DigitizerSpecifications Digitizer { set; get; } = new();

        /// <summary>
        /// Specifications for the tablet's pen.
        /// </summary>
        [JsonRequired, JsonProperty(nameof(Pen))]
        [Required(ErrorMessage = $"{nameof(Pen)} specifications must be defined")]
        public required PenSpecifications Pen { set; get; } = new();

        /// <summary>
        /// Specifications for the auxiliary buttons.
        /// </summary>
        [JsonProperty(nameof(AuxiliaryButtons))]
        public ButtonSpecifications? AuxiliaryButtons { set; get; }

        /// <summary>
        /// Specifications for the mouse buttons.
        /// </summary>
        [JsonProperty(nameof(MouseButtons))]
        public ButtonSpecifications? MouseButtons { set; get; }

        /// <summary>
        /// Specifications for the wheels.
        /// </summary>
        [JsonProperty(nameof(Wheel))]
        public WheelSpecifications? Wheel { get; set; }

        /// <summary>
        /// Specifications for the strips.
        /// <b>NOTE:</b> This is not a complete feature and might change in the future
        /// </summary>
        public AnalogSpecifications? Strips { set; get; }

        /// <summary>
        /// Specifications for the touch digitizer.
        /// </summary>
        [JsonProperty(nameof(Touch))]
        public DigitizerSpecifications? Touch { set; get; }
    }
}
