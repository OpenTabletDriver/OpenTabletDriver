using System.Numerics;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// An absolute positioned device report.
    /// </summary>
    public interface IAbsolutePositionReport : IDeviceReport
    {
        /// <summary>
        /// The absolute position of the pen.
        /// </summary>
        Vector2 Position { get; set; }
    }
}
