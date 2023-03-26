using System.ComponentModel;
using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// Device specifications for a wheel.
    /// </summary>
    [PublicAPI]
    public class WheelSpecifications
    {
        /// <summary>
        /// The amount of steps making up a whole rotation.
        /// </summary>
        [DisplayName("Steps")]
        public uint StepCount { set; get; }

        /// <summary>
        /// Does the wheel report relative position (movement) or absolute position
        /// </summary>
        [DisplayName("Relative")]
        public bool IsRelative { get; set; }

        /// <summary>
        /// Does a increment/positive movement indicate clockwise movement?
        /// </summary>
        [DisplayName("Clockwise")]
        public bool IsClockwise { get; set; }

        /// <summary>
        /// For relative wheels, the angle on the unit circle, corresponding to a reading of zero from the sensor
        /// </summary>
        public float AngleOfZeroReading { get; set; }
    }
}
