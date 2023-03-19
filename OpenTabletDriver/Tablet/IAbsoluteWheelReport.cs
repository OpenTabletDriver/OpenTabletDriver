using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// An auxiliary report containing states of express keys.
    /// </summary>
    [PublicAPI]
    public interface IAbsoluteWheelReport : IDeviceReport
    {
        /// <summary>
        /// The position of the wheel
        /// </summary>
        int? WheelPosition { get; set; }
    }
}
