using System;
using HidSharp;


namespace TabletDriverLib.Interop.HID
{
    public class WindowsDeviceSeizer : IDeviceSeizer
    {
        public unsafe WindowsDeviceSeizer()
        {

        }

        public void Dispose()
        {

        }

        public bool IsSupported()
        {
            return false;
        }

        public void Seize(HidStream device)
        {
            throw new NotImplementedException();
        }


    }
}
