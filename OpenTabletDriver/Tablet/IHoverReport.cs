using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// A report containing information for pen hover.
    /// </summary>
    [PublicAPI]
    public interface IHoverReport : IDeviceReport
    {
        /// <summary>
        /// The hover distance of the pen. This is an arbitrary value with no defined real-world units.
        /// </summary>
        uint HoverDistance { set; get; }
    }
}
