using System.ComponentModel;
using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// Device specifications for a pen tool.
    /// </summary>
    [PublicAPI]
    public class PenSpecifications : ButtonSpecifications
    {
        /// <summary>
        /// The maximum pressure that the pen supports.
        /// </summary>
        [DisplayName("Max Pressure")]
        public uint MaxPressure { set; get; }
    }
}
