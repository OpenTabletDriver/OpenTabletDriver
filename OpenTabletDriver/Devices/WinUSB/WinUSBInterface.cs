using System;
using System.Buffers;
using System.IO;
using System.Threading;
using OpenTabletDriver.Native.Windows;
using OpenTabletDriver.Native.Windows.USB;
using static OpenTabletDriver.Native.Windows.Windows;
using static OpenTabletDriver.Native.Windows.WinUsb;

namespace OpenTabletDriver.Devices.WinUSB
{
    internal class WinUSBInterface : IDeviceEndpoint
    {
        public unsafe WinUSBInterface(string devicePath)
        {
            DevicePath = devicePath;
            WithHandle(winUsbHandle =>
            {
                var packet = SetupPacket.MakeGetDescriptor(
                    RequestInternalType.Standard,
                    RequestRecipient.Device,
                    DescriptorType.Device, 0,
                    (ushort)sizeof(DeviceDescriptor)
                );

                var deviceDescriptor = new DeviceDescriptor();
                void* descriptorPtr = &deviceDescriptor;

                if (!WinUsb_ControlTransfer(winUsbHandle!, packet, descriptorPtr, (uint)sizeof(DeviceDescriptor), out _, null))
                    throw new IOException("Failed to retrieve device descriptor");

                var interfaceDescriptor = new InterfaceDescriptor();
                if (!WinUsb_QueryInterfaceSettings(winUsbHandle!, 0, &interfaceDescriptor))
                    throw new IOException("Failed to get interface descriptor");

                InterfaceNum = interfaceDescriptor.bInterfaceNumber;

                for (byte i = 0; i < interfaceDescriptor.bNumEndpoints; i++)
                {
                    var b = i; // Workaround CoreCLR bug
                    if (!WinUsb_QueryPipe(winUsbHandle!, 0, i, out var pipeInfo))
                        throw new IOException("Failed to get pipe information");
                    i = b;

                    var direction = (PipeDirection)((pipeInfo.PipeID >> 7) & 1);
                    switch (direction)
                    {
                        case PipeDirection.Out:
                            if (OutputPipe.HasValue)
                                throw new IOException("WinUSB HID device unexpectedly have more than one output endpoint on the same interface");

                            OutputPipe = pipeInfo.PipeID;
                            OutputReportLength = pipeInfo.MaximumPacketSize;
                            break;
                        case PipeDirection.In:
                            if (InputPipe.HasValue)
                                throw new IOException("WinUSB HID device unexpectedly have more than one input endpoint on the same interface");

                            InputPipe = pipeInfo.PipeID;
                            InputReportLength = pipeInfo.MaximumPacketSize;
                            break;
                    }
                }

                ProductID = deviceDescriptor.idProduct;
                VendorID = deviceDescriptor.idVendor;

                Manufacturer = deviceDescriptor.iManufacturer != 0
                    ? GetDeviceString(deviceDescriptor.iManufacturer) ?? string.Empty
                    : "Unknown Manufacturer";

                ProductName = deviceDescriptor.iProduct != 0
                    ? GetDeviceString(deviceDescriptor.iProduct) ?? string.Empty
                    : "Unknown Product Name";

                SerialNumber = deviceDescriptor.iSerialNumber != 0
                    ? GetDeviceString(deviceDescriptor.iSerialNumber)
                    : "Unknown Serial Number";
            });
        }

        private int _referenceCount;
        private SafeFileHandle? _activeFileHandle;
        private SafeWinUsbInterfaceHandle? _activeWinUsbHandle;

        internal int InterfaceNum { private set; get; }
        internal byte? InputPipe { private set; get; }
        internal byte? OutputPipe { private set; get; }

        public int ProductID { private set; get; }

        public int VendorID { private set; get; }

        public int InputReportLength { private set; get; }

        public int OutputReportLength { private set; get; }

        public int FeatureReportLength => 0; // requires parsing report descriptor to determine feature report length

        public string? Manufacturer { private set; get; }

        public string? ProductName { private set; get; }

        public string? FriendlyName => ProductName;

        public string? SerialNumber { private set; get; }

        public string DevicePath { get; }

        public bool CanOpen => true;

        public unsafe string? GetDeviceString(byte index)
        {
            return WithHandle(winUsbHandle =>
            {
                var packet = SetupPacket.MakeGetStringDescriptor(index);
                var buffer = ArrayPool<byte>.Shared.Rent(StringDescriptor.MaxSize);

                fixed (void* bufferPtr = &buffer[0])
                {
                    if (!WinUsb_ControlTransfer(winUsbHandle!, packet, bufferPtr, StringDescriptor.MaxSize, out var descriptorLength, null))
                    {
                        ArrayPool<byte>.Shared.Return(buffer);
                        return null;
                    }

                    ArrayPool<byte>.Shared.Return(buffer);
                    return StringDescriptor.GetString((StringDescriptor*)bufferPtr);
                }
            });
        }

        public IDeviceEndpointStream Open()
        {
            return new WinUSBInterfaceStream(new WeakReference<WinUSBInterface>(this));
        }

        private void WithHandle(Action<SafeWinUsbInterfaceHandle?> predicate)
        {
            var handle = BorrowHandle();
            try
            {
                predicate(handle);
            }
            finally
            {
                ReturnHandle(handle);
            }
        }

        private T WithHandle<T>(Func<SafeWinUsbInterfaceHandle?, T> predicate)
        {
            var handle = BorrowHandle();
            try
            {
                return predicate(handle);
            }
            finally
            {
                ReturnHandle(handle);
            }
        }

        internal SafeWinUsbInterfaceHandle? BorrowHandle()
        {
            if (Interlocked.Increment(ref _referenceCount) == 1)
            {
                _activeFileHandle = CreateFile(DevicePath,
                    FileAccess.ReadWrite,
                    FileShare.ReadWrite,
                    IntPtr.Zero,
                    FileMode.Open,
                    (FileAttributes)0x40000000,
                    IntPtr.Zero
                );

                if (_activeFileHandle.IsInvalid)
                    throw new IOException("Failed to open file handle to WinUSB interface");

                if (!WinUsb_Initialize(_activeFileHandle, out _activeWinUsbHandle))
                    throw new IOException("Failed to initialize WinUSB interface");
            }

            return _activeWinUsbHandle;
        }

        // Take reference, so we may easily add multiple interface support in the future
        internal void ReturnHandle(SafeWinUsbInterfaceHandle? handle)
        {
            if (_activeWinUsbHandle != handle)
                throw new InvalidOperationException("Returning handle is not equal to active handle");

            if (Interlocked.Decrement(ref _referenceCount) == 0)
            {
                _activeWinUsbHandle?.Dispose();
                _activeFileHandle?.Dispose();

                _activeWinUsbHandle = null;
                _activeFileHandle = null;
            }
        }

        public bool IsSibling(IDeviceEndpoint other)
        {
            // TODO: For now we assume that all WinUSB interfaces are not siblings.
            //       Can probably be implemented with CM_Get_Parent until a USB device is found.
            return false;
        }
    }
}
