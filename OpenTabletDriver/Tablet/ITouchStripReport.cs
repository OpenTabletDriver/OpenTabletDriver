using System.Numerics;
using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// A device report containing a touch strip (or touch ring) for the tablet.
    /// </summary>
    [PublicAPI]
    public interface ITouchStripReport : IDeviceReport
    {
        /// <summary>
        /// The current touch directions of all touch strips.
        /// </summary>
        TouchStripDirection[] TouchStripDirections { get; set; }
    }
}
