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
    }
}
