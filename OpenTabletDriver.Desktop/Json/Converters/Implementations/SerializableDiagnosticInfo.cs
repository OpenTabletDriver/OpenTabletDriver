using System;
using System.Collections.Generic;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Logging;

namespace OpenTabletDriver.Desktop.Json.Converters.Implementations
{
    internal sealed class SerializableDiagnosticInfo : Serializable, IDiagnosticInfo
    {
        public string AppVersion { set; get; }
        public string BuildDate { set; get; }
        public OperatingSystem OperatingSystem { set; get; }
        public IDictionary<string, string> EnvironmentVariables { set; get; }
        public IEnumerable<IDeviceEndpoint> Devices { set; get; }
        public IEnumerable<LogMessage> ConsoleLog { set; get; }
    }
}
