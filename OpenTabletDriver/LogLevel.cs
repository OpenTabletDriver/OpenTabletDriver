using JetBrains.Annotations;
using OpenTabletDriver.Logging;

namespace OpenTabletDriver
{
    /// <summary>
    /// Various severity levels for <see cref="LogMessage"/>s.
    /// </summary>
    [PublicAPI]
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }
}
