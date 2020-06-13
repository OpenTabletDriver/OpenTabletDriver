using HidSharp;

namespace TabletDriverLib.Interop.HID
{
    public interface IDeviceSeizer
    {
        void Seize(HidStream device);
        bool IsSupported();
    }
}