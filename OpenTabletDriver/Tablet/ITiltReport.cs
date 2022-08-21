using System.Numerics;
using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// A device report containing a tilt vector for the pen.
    /// </summary>
    [PublicAPI]
    public interface ITiltReport : IDeviceReport
    {
        /// <summary>
        /// The current tilt angle of the pen. This is an arbitrary value with no defined real-world units.
        /// </summary>
        Vector2 Tilt { set; get; }
    }
}
