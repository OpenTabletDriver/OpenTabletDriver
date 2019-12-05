using TabletDriverLib.Tablet;

namespace TabletDriverLib.Compatibility
{
    public interface ICompatibilityLayer<T> where T : IDeviceReport
    {
        T Fix(byte[] report);
    }
}