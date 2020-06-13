using System;
using HidSharp;


namespace TabletDriverLib.Interop.HID
{
    public class EvdevDeviceSeizer : IDeviceSeizer, IDisposable
    {
        public unsafe EvdevDeviceSeizer()
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
