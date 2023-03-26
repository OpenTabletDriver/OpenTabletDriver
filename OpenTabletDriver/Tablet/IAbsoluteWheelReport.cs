using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// An auxiliary report containing states of a wheel/ring input.
    /// </summary>
    [PublicAPI]
    public interface IAbsoluteWheelReport : IDeviceReport
    {
        /// <summary>
        /// The position reading of the wheel, or null to indicate an absence of touch.
        /// </summary>
        uint? WheelPosition { get; set; }
    }
}
