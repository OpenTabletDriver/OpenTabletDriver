using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using OpenTabletDriver.Native.Windows.USB;

namespace OpenTabletDriver.Native.Windows
{
    public static class WinUsb
    {
        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_Initialize(SafeFileHandle handle, out SafeWinUsbInterfaceHandle interfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_Free(IntPtr interfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        public static unsafe extern bool WinUsb_ReadPipe(SafeWinUsbInterfaceHandle interfaceHandle, byte pipeId, void* pBuffer, uint bufferLength, out uint lengthTransferred, NativeOverlapped* pOverlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        public static unsafe extern bool WinUsb_WritePipe(SafeWinUsbInterfaceHandle interfaceHandle, byte pipeId, void* pBuffer, uint bufferLength, out uint lengthTransferred, NativeOverlapped* pOverlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        public static unsafe extern bool WinUsb_ControlTransfer(SafeWinUsbInterfaceHandle interfaceHandle, SetupPacket setupPacket, void* pBuffer, uint bufferLength, out uint lengthTransferred, NativeOverlapped* pOverlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        public static unsafe extern bool WinUsb_QueryInterfaceSettings(SafeWinUsbInterfaceHandle interfaceHandle, byte altInterfaceNum, InterfaceDescriptor* interfaceDescriptor);

        [DllImport("winusb.dll", SetLastError = true)]
        public static extern bool WinUsb_QueryPipe(SafeWinUsbInterfaceHandle interfaceHandle, byte altInterfaceNum, byte pipeIndex, out PipeInfo pipeInfo);
    }
}
