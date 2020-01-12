using TabletDriverPlugin.Tablet;

namespace TabletDriverPlugin
{
    public interface IOutputMode
    {
        void Read(IDeviceReport report);
    }
}