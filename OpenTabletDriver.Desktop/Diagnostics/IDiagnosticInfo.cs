using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Logging;

#nullable enable

namespace OpenTabletDriver.Desktop.Diagnostics
{
    [PublicAPI]
    public interface IDiagnosticInfo
    {
        string AppVersion { get; }
        string BuildDate { get; }
        OperatingSystem OperatingSystem { get; }
        IDictionary<string, string> EnvironmentVariables { get; }
        IEnumerable<IDeviceEndpoint> Devices { get; }
        IEnumerable<LogMessage> ConsoleLog { get; }
    }
}
