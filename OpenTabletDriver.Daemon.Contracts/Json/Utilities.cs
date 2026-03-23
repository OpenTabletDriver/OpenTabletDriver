using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenTabletDriver.Daemon.Contracts.Json.Converters;
using OpenTabletDriver.Daemon.Contracts.Json.Converters.Implementations;
using OpenTabletDriver.Devices;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Daemon.Contracts.Json
{
    internal static class Utilities
    {
        internal static readonly JsonConverter[] Converters = new JsonConverter[]
        {
            new InterfaceConverter<IReportParser<IDeviceReport>, SerializableDeviceReportParser>(),
            new InterfaceConverter<IDeviceEndpoint, SerializableDeviceEndpoint>(),
            new InterfaceConverter<IDeviceEndpointStream, SerializableDeviceEndpointStream>(),
            new InterfaceConverter<IDisplay, SerializableDisplay>(),
            new VersionConverter()
        };
    }
}
