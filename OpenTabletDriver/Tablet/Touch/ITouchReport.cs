using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet.Touch
{
    /// <summary>
    /// A device report containing touch input.
    /// </summary>
    [PublicAPI]
    public interface ITouchReport : IDeviceReport
    {
        /// <summary>
        /// The absolutely positioned touch points.
        /// </summary>
        TouchPoint?[] Touches { set; get; }
    }
}
