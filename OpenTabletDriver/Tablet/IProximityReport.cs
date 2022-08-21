using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// A report containing information for pen proximity.
    /// </summary>
    [PublicAPI]
    public interface IProximityReport : IDeviceReport
    {
        /// <summary>
        /// Whether the pen is in proximity.
        /// </summary>
        bool NearProximity { set; get; }

        /// <summary>
        /// The hover distance of the pen. This is an arbitrary value with no defined real-world units.
        /// </summary>
        uint HoverDistance { set; get; }
    }
}
