using System.Collections.Generic;
using JetBrains.Annotations;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Logging;

namespace OpenTabletDriver.Desktop.Diagnostics
{
    [PublicAPI]
    public interface IDiagnosticInfo
    {
        string AppVersion { get; }
        string BuildDate { get; }
        OSInfo OperatingSystem { get; }
        IDictionary<string, string> EnvironmentVariables { get; }
        IEnumerable<IDeviceEndpoint> Devices { get; }
        IEnumerable<LogMessage> ConsoleLog { get; }
    }
}
