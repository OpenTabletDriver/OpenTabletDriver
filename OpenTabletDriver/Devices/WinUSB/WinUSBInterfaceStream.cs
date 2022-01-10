using System;
using System.IO;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Native.Windows.USB;
using OpenTabletDriver.Plugin.Devices;
using static OpenTabletDriver.Native.Windows.WinUsb;

namespace OpenTabletDriver.Devices.WinUSB
{
    public unsafe class WinUSBInterfaceStream : IDeviceEndpointStream
    {
        private readonly WeakReference<WinUSBInterface> parentInterface;
        private readonly int interfaceNum;
        private readonly SafeWinUsbInterfaceHandle winUsbHandle;
        private readonly byte readPipe;
        private readonly byte writePipe;
        private readonly byte[] readBuffer;
        private readonly byte* readPtr;
        private readonly byte[] writeBuffer;
        private readonly byte* writePtr;

        public WinUSBInterfaceStream(WeakReference<WinUSBInterface> parentInterface)
        {
            this.parentInterface = parentInterface;

            if (!parentInterface.TryGetTarget(out var usbInterface))
                throw new InvalidOperationException("Weak reference to parent interface is unexpectedly invalid");

            this.interfaceNum = usbInterface.InterfaceNum;

            winUsbHandle = usbInterface.BorrowHandle();

            if (usbInterface.InputPipe is byte readPipe)
            {
                this.readPipe = readPipe;
                readBuffer = GC.AllocateArray<byte>(usbInterface.InputReportLength, true);
                readPtr = (byte*)Unsafe.AsPointer(ref readBuffer[0]);
            }

            if (usbInterface.OutputPipe is byte writePipe)
            {
                this.writePipe = writePipe;
                writeBuffer = GC.AllocateArray<byte>(usbInterface.OutputReportLength, true);
                writePtr = (byte*)Unsafe.AsPointer(ref writeBuffer[0]);
            }
        }

        public byte[] Read()
        {
            WinUsb_ReadPipe(winUsbHandle, readPipe, readPtr, (uint)readBuffer.Length, out var bytesRead, null);
            return bytesRead < readBuffer.Length
                ? readBuffer.AsSpan(0, (int)bytesRead).ToArray()
                : readBuffer;
        }

        public void Write(byte[] buffer)
        {
            if (buffer.Length < writeBuffer.Length)
            {
                writeBuffer.AsSpan().Clear();
                buffer.AsSpan().CopyTo(writeBuffer);
                WinUsb_WritePipe(winUsbHandle, writePipe, writePtr, (uint)writeBuffer.Length, out _, null);
            }
            else
            {
                fixed (void* bufferPtr = &buffer[0])
                {
                    WinUsb_WritePipe(winUsbHandle, writePipe, bufferPtr, (uint)buffer.Length, out _, null);
                }
            }
        }

        public unsafe void GetFeature(byte[] buffer)
        {
            var length = buffer.Length; // requires HID report descriptor parsing to implement properly, assume caller is correct for now
            var packet = new SetupPacket()
            {
                bmRequestType = new RequestType(RequestDirection.DeviceToHost, RequestInternalType.Class, RequestRecipient.Interface),
                bRequest = 0x01, // GET_REPORT
                wValue = (ushort)((buffer[0] & 0xff) | (0x0300)),
                wIndex = (ushort)interfaceNum,
                wLength = (ushort)length
            };

            fixed (void* bufferPtr = &buffer[0])
            {
                WinUsb_ControlTransfer(winUsbHandle, packet, bufferPtr, (uint)length, out _, null);
            }
        }

        public void SetFeature(byte[] buffer)
        {
            var length = buffer.Length; // requires HID report descriptor parsing to implement properly, assume caller is correct for now
            var packet = new SetupPacket()
            {
                bmRequestType = new RequestType(RequestDirection.HostToDevice, RequestInternalType.Class, RequestRecipient.Interface),
                bRequest = 0x09, // SET_REPORT
                wValue = (ushort)((buffer[0] & 0xff) | (0x0300)),
                wIndex = (ushort)interfaceNum,
                wLength = (ushort)length
            };

            fixed (void* bufferPtr = &buffer[0])
            {
                WinUsb_ControlTransfer(winUsbHandle, packet, bufferPtr, (uint)length, out _, null);
            }
        }

        public void Dispose()
        {
            if (parentInterface.TryGetTarget(out var usbInterface))
                usbInterface.ReturnHandle(winUsbHandle);
        }
    }
}
