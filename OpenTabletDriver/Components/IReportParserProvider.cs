using JetBrains.Annotations;
using OpenTabletDriver.Tablet;

#nullable enable

namespace OpenTabletDriver.Components
{
    /// <summary>
    /// Provides a provider for all supported report parsers.
    /// </summary>
    [PublicAPI]
    public interface IReportParserProvider
    {
        /// <summary>
        /// Requests a device report parser by a specific type path.
        /// </summary>
        /// <param name="reportParserName">The type path for the report parser.</param>
        /// <returns>An instance of the requested device report parser.</returns>
        IReportParser<IDeviceReport> GetReportParser(string reportParserName);
    }
}
