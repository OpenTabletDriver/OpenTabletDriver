using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// A report containing battery information.
    /// </summary>
    [PublicAPI]
    public interface IBatteryReport  : IDeviceReport
    {
        /// <summary>
        /// Charge percentage of device
        /// </summary>
        uint ChargePercent { set; get; }
        /// <summary>
        /// Whether or not the device is plugged in
        /// </summary>
        bool PluggedIn { set; get; }
    }
}
