using System;
using System.Runtime.InteropServices;
using NativeLib.OSX.Generic;

namespace NativeLib.OSX
{
    using IOReturn = UInt32;
    using static Utility;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe public delegate IOReturn DeviceRequest(IOUSBDeviceInterface** self, ref IOUSBDevRequest req);


    [StructLayout(LayoutKind.Sequential)]
    public struct IOUSBDeviceInterface 
    {
        public IUnknownCGuts guts;

        IntPtr CreateDeviceAsyncEventSource;
        IntPtr GetDeviceAsyncEventSource;
        IntPtr CreateDeviceAsyncPort;
        IntPtr GetDeviceAsyncPort;
        IntPtr USBDeviceOpen;
        IntPtr USBDeviceClose;
        IntPtr GetDeviceClass;
        IntPtr GetDeviceSubClass;
        IntPtr GetDeviceProtocol;
        IntPtr GetDeviceVendor;
        IntPtr GetDeviceProduct;
        IntPtr GetDeviceReleaseNumber;
        IntPtr GetDeviceAddress;
        IntPtr GetDeviceBusPowerAvailable;
        IntPtr GetDeviceSpeed;
        IntPtr GetNumberOfConfigurations;
        IntPtr GetLocationID;
        IntPtr GetConfigurationDescriptorPtr;
        IntPtr GetConfiguration;
        IntPtr SetConfiguration;
        IntPtr GetBusFrameNumber;
        IntPtr ResetDevice;


        IntPtr DeviceRequestPtr;
        public DeviceRequest DeviceRequest => Wrap<DeviceRequest>(DeviceRequestPtr);
    }
}
