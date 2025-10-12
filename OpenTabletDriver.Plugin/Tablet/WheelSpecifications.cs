using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OpenTabletDriver.Plugin.Tablet
{
    /// <summary>
    /// Device specifications for a wheel.
    /// </summary>
    public class WheelSpecifications : AnalogSpecifications
    {
        /// <summary>
        /// For Absolute Wheels, The physical angle on the wheel's unit circle, corresponding to a reading of zero
        /// </summary>
        [Required(ErrorMessage = $"{nameof(AngleOfZeroReading)} must be defined")]
        [Range(0, 360)]
        public float AngleOfZeroReading { get; set; }

        /// <summary>
        /// Specifications for the wheel buttons
        /// </summary>
        [Required(ErrorMessage = $"{nameof(Buttons)} must be defined")]
        public ButtonSpecifications Buttons { set; get; } = new ButtonSpecifications();
    }
}
