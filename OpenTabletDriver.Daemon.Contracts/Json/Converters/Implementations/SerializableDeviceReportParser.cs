using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Daemon.Contracts.Json.Converters.Implementations
{
    internal sealed class SerializableDeviceReportParser : Serializable, IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report) => throw NotSupported();
    }
}
