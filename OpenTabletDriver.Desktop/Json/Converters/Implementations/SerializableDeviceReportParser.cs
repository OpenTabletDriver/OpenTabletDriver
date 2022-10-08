using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.Json.Converters.Implementations
{
    internal sealed class SerializableDeviceReportParser : Serializable, IReportParser<IDeviceReport>
    {
        public IDeviceReport Parse(byte[] report) => throw NotSupported();
    }
}
