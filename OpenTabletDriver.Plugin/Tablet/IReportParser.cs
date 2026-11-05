using System;

namespace OpenTabletDriver.Plugin.Tablet
{
    /// <summary>
    /// Defines a tablet report parser that is dynamically looked up when
    /// defined in <see cref="TabletConfiguration"/> in <see cref="DeviceIdentifier.ReportParser"/>
    /// </summary>
    public interface IReportParser
    {
        /// <summary>
        /// Parse the report bytes into the <see cref="DeviceReport"/>
        /// </summary>
        /// <param name="raw">A span of bytes containing the report</param>
        /// <param name="deviceReport">An initialized device report</param>
        /// <returns></returns>
        // TODO: <in> on <raw> might not be necessary
        bool Parse(scoped in ReadOnlySpan<byte> raw, scoped ref DeviceReport deviceReport);
    }
}
