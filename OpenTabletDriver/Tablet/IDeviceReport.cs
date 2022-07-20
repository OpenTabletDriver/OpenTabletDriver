using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// A device report.
    /// </summary>
    [PublicAPI]
    public interface IDeviceReport
    {
        /// <summary>
        /// The raw data forming the device report.
        /// </summary>
        byte[] Raw { set; get; }
    }
}
