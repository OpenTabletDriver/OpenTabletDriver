using System;
using NativeLib.OSX;

namespace TabletDriverLib.Interop.USB
{
    using static NativeLib.OSX.OSX;

    public class MacoSUSBUtility : IUSBUtility
    {
        public bool InitStrings(string hidDvicePath, byte[] array)
        {
            bool success = false;
            unsafe
            {
                var hidEntry = IORegistryEntryFromPath(0, hidDvicePath);
                uint interfaceEntry = 0;
                uint deviceEntry = 0;
                IUnknownCGuts** iodev = null;
                IOUSBDeviceInterface** usbdev = null;
                if (hidEntry != 0)
                {
                    IORegistryEntryGetParentEntry(hidEntry, kIOServicePlane, ref interfaceEntry);
                    if (interfaceEntry != 0)
                    {
                        IORegistryEntryGetParentEntry(interfaceEntry, kIOServicePlane, ref deviceEntry);
                    }
                }
                if (deviceEntry != 0)
                {
                    Int32 score = 0;

                    IOCreatePlugInInterfaceForService(deviceEntry,
                        kIOUSBDeviceUserClientTypeID, kIOCFPlugInInterfaceID,
                        ref iodev, ref score);
                    if (iodev != null)
                    {
                        IntPtr devPtr = IntPtr.Zero;

                        (**iodev).QueryInterface(iodev, CFUUIDGetUUIDBytes(kIOUSBDeviceInterfaceID), ref devPtr);

                        if (devPtr != IntPtr.Zero)
                        {
                            usbdev = (IOUSBDeviceInterface**) devPtr.ToPointer();
                            var bLength = 84;
                            var buffer = new byte[bLength + 2];
                            fixed(byte* p = buffer)
                            {
                                success = true;
                                for(int i = 0; i < array.Length; i++)
                                {
                                    IOUSBDevRequest request;
                                    request.bmRequestType = 128; //USBmakebmRequestType(kUSBIn, kUSBStandard, kUSBDevice);
                                    request.bRequest = 6; //kUSBRqGetDescriptor
                                    request.wValue = (ushort)(3 << 8 | array[i]);
                                    request.wIndex = 0;
                                    request.wLength = (ushort)bLength;
                                    request.pData = new IntPtr((void*)p);
                                    request.wLenDone = 0;
                                    var result = (**usbdev).DeviceRequest(usbdev, ref request);
                                    if (result != 0) success = false;
                                }
                            }
                        }
                    }

                    if(hidEntry != 0)
                        IOObjectRelease(hidEntry);
                    if (interfaceEntry != 0)
                        IOObjectRelease(interfaceEntry);
                    if (deviceEntry != 0)
                        IOObjectRelease(deviceEntry);
                    if(usbdev != null)
                        (**usbdev).guts.Release(new IntPtr(usbdev));
                    if (iodev != null)
                        (**iodev).Release(new IntPtr(iodev));
                }
                return success;
            }
        }
    }
}