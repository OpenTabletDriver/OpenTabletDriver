using System;
using System.Reflection;
using HidSharp;
using NativeLib.OSX;
using TabletDriverPlugin;

namespace TabletDriverLib.Interop.HID
{
    public class MacOSDeviceSeizer : IDeviceSeizer
    {

        public unsafe MacOSDeviceSeizer()
        {

        }


        public bool IsSupported()
        {
            return true;
        }

        public void Seize(HidStream device)
        {
            var IODevice = (IntPtr)device
                .GetType()
                .GetField("_handle", BindingFlags.NonPublic| BindingFlags.Instance | BindingFlags.GetField)
                .GetValue(device);
            if(IODevice != IntPtr.Zero)
            {
                var sucess = OSX.IOHIDDeviceClose(IODevice, IOHIDOptionsType.kIOHIDOptionsTypeNone);
                if(sucess == 0)
                {
                    sucess = OSX.IOHIDDeviceOpen(IODevice, IOHIDOptionsType.kIOHIDOptionsTypeSeizeDevice);
                }
                //what shoud we
            }


        }


    }
}
