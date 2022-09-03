using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// A device report parser.
    /// </summary>
    /// <typeparam name="T">
    /// The parsed report type.
    /// </typeparam>
    [PublicAPI]
    public interface IReportParser<out T> where T : IDeviceReport
    {
        /// <summary>
        /// Parses a device report from raw data.
        /// </summary>
        /// <param name="report">The report data to parse.</param>
        /// <returns>A parsed report of <see cref="T"/>.</returns>
        T Parse(byte[] report);
    }
}
