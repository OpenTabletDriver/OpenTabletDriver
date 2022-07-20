using System;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Native.Windows.USB;
using static OpenTabletDriver.Native.Windows.WinUsb;

namespace OpenTabletDriver.Devices.WinUSB
{
    internal unsafe class WinUSBInterfaceStream : IDeviceEndpointStream
    {
        private readonly WeakReference<WinUSBInterface> _parentInterface;
        private readonly int _interfaceNum;
        private readonly SafeWinUsbInterfaceHandle? _winUsbHandle;
        private readonly byte _readPipe;
        private readonly byte _writePipe;
        private readonly byte[] _readBuffer = Array.Empty<byte>();
        private readonly byte* _readPtr;
        private readonly byte[] _writeBuffer = Array.Empty<byte>();
        private readonly byte* _writePtr;

        public WinUSBInterfaceStream(WeakReference<WinUSBInterface> parentInterface)
        {
            _parentInterface = parentInterface;

            if (!parentInterface.TryGetTarget(out var usbInterface))
                throw new InvalidOperationException("Weak reference to parent interface is unexpectedly invalid");

            _interfaceNum = usbInterface.InterfaceNum;

            _winUsbHandle = usbInterface.BorrowHandle();

            if (usbInterface.InputPipe is byte readPipe)
            {
                _readPipe = readPipe;
                _readBuffer = GC.AllocateArray<byte>(usbInterface.InputReportLength, true);
                _readPtr = (byte*)Unsafe.AsPointer(ref _readBuffer[0]);
            }

            if (usbInterface.OutputPipe is byte writePipe)
            {
                _writePipe = writePipe;
                _writeBuffer = GC.AllocateArray<byte>(usbInterface.OutputReportLength, true);
                _writePtr = (byte*)Unsafe.AsPointer(ref _writeBuffer[0]);
            }
        }

        public byte[] Read()
        {
            WinUsb_ReadPipe(_winUsbHandle, _readPipe, _readPtr, (uint)_readBuffer.Length, out var bytesRead, null);
            return bytesRead < _readBuffer.Length
                ? _readBuffer.AsSpan(0, (int)bytesRead).ToArray()
                : _readBuffer;
        }

        public void Write(byte[] buffer)
        {
            if (buffer.Length < _writeBuffer.Length)
            {
                _writeBuffer.AsSpan().Clear();
                buffer.AsSpan().CopyTo(_writeBuffer);
                WinUsb_WritePipe(_winUsbHandle, _writePipe, _writePtr, (uint)_writeBuffer.Length, out _, null);
            }
            else
            {
                fixed (void* bufferPtr = &buffer[0])
                {
                    WinUsb_WritePipe(_winUsbHandle, _writePipe, bufferPtr, (uint)buffer.Length, out _, null);
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
                wIndex = (ushort)_interfaceNum,
                wLength = (ushort)length
            };

            fixed (void* bufferPtr = &buffer[0])
            {
                WinUsb_ControlTransfer(_winUsbHandle, packet, bufferPtr, (uint)length, out _, null);
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
                wIndex = (ushort)_interfaceNum,
                wLength = (ushort)length
            };

            fixed (void* bufferPtr = &buffer[0])
            {
                WinUsb_ControlTransfer(_winUsbHandle, packet, bufferPtr, (uint)length, out _, null);
            }
        }

        public void Dispose()
        {
            if (_parentInterface.TryGetTarget(out var usbInterface))
                usbInterface.ReturnHandle(_winUsbHandle);
        }
    }
}
