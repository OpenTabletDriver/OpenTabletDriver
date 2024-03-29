using System.Collections.Generic;
using Newtonsoft.Json;
using OpenTabletDriver.Daemon.Contracts.Json.Converters;
using OpenTabletDriver.Daemon.Contracts.Json.Converters.Implementations;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Daemon.Contracts.Json
{
    internal static class Utilities
    {
        internal static readonly IEnumerable<JsonConverter> Converters = new JsonConverter[]
        {
            new InterfaceConverter<IReportParser<IDeviceReport>, SerializableDeviceReportParser>(),
            new InterfaceConverter<IDeviceEndpoint, SerializableDeviceEndpoint>(),
            new InterfaceConverter<IDeviceEndpointStream, SerializableDeviceEndpointStream>(),
            new InterfaceConverter<IDisplay, SerializableDisplay>()
        };
    }
}
