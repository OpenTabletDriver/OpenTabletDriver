using System.Collections.Generic;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Logging;

namespace OpenTabletDriver.Desktop.Json.Converters.Implementations
{
    internal sealed class SerializableDiagnosticInfo : Serializable, IDiagnosticInfo
    {
        public string AppVersion { set; get; } = string.Empty;
        public string BuildDate { set; get; } = string.Empty;
        public OSInfo OperatingSystem { set; get; } = null!;
        public IDictionary<string, string> EnvironmentVariables { set; get; } = null!;
        public IEnumerable<IDeviceEndpoint> Devices { set; get; } = null!;
        public IEnumerable<LogMessage> ConsoleLog { set; get; } = null!;
    }
}
