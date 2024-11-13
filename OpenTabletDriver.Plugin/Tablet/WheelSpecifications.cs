using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OpenTabletDriver.Plugin.Tablet
{
    /// <summary>
    /// Device specifications for a wheel.
    /// </summary>
    public class WheelSpecifications
    {
        /// <summary>
        /// The amount of steps making up a whole rotation.
        /// </summary>
        [Required(ErrorMessage = $"{nameof(StepCount)} must be defined")]
        public uint StepCount { set; get; }

        /// <summary>
        /// Does the wheel report relative position (movement) or absolute position
        /// </summary>
        [Required(ErrorMessage = $"{nameof(IsRelative)} must be defined")]
        public bool IsRelative { get; set; }

        /// <summary>
        /// Does a increment/positive movement indicate clockwise movement?
        /// </summary>
        [Required(ErrorMessage = $"{nameof(IsClockwise)} must be defined")]
        public bool IsClockwise { get; set; }

        /// <summary>
        /// For absolute wheels, the angle on the unit circle, corresponding to a reading of zero from the sensor
        /// </summary>
        [Required(ErrorMessage = $"{nameof(AngleOfZeroReading)} must be defined")]
        [Range(0, 360)]
        public float AngleOfZeroReading { get; set; }
    }
}
