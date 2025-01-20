using System.ComponentModel.DataAnnotations;

namespace OpenTabletDriver.Plugin.Tablet
{
    /// <summary>
    /// Device specifications for a an analog reporting device, such as knobs, wheels & strips (more commonly touch strips).
    /// </summary>
    /// <remarks>
    /// This spec currently does not allow analog devices with different specs.
    /// </remarks>
    public class AnalogSpecifications
    {
        /// <summary>
        /// The amount of steps in the analog device.
        /// </summary>
        [Required(ErrorMessage = $"{nameof(StepCount)} must be defined")]
        public uint StepCount { set; get; }

        /// <summary>
        /// Does the device report relative position (movement) or absolute position
        /// </summary>
        [Required(ErrorMessage = $"{nameof(IsRelative)} must be defined")]
        public bool IsRelative { get; set; }
    }
}
