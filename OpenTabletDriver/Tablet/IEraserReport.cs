using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// An report designating the pen eraser state.
    /// </summary>
    [PublicAPI]
    public interface IEraserReport : IDeviceReport
    {
        /// <summary>
        /// Whether the eraser is the pen side in range.
        /// </summary>
        bool Eraser { set; get; }
    }
}
