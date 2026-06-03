using System;
using System.Runtime.CompilerServices;
using HidSharp;
using OpenTabletDriver.Logging;

namespace OpenTabletDriver.Daemon.Library.Output.WindowsInk
{
    internal class VMultiInstance
    {
        private readonly HidStream? _device;
        protected readonly byte[] Buffer;

        public unsafe VMultiReportHeader* Header { get; }
        public bool Extended { get; }

        public unsafe VMultiInstance(string name, int size)
        {
            Buffer = GC.AllocateArray<byte>(size, pinned: true);
            Header = (VMultiReportHeader*)Unsafe.AsPointer(ref Buffer[0]);
            _device = Retrieve(name, out var extended);
            Extended = extended;
        }

        public void Write()
        {
            _device?.Write(Buffer);
        }

        public unsafe void EnableButtonBit(int bit)
        {
            Header->Buttons = (byte)(Header->Buttons | bit);
        }

        public unsafe void DisableButtonBit(int bit)
        {
            Header->Buttons = (byte)(Header->Buttons & ~bit);
        }

        public static bool HasBit(byte buttons, int bit)
        {
            return (buttons & bit) != 0;
        }

        private static HidStream? Retrieve(string name, out bool extended)
        {
            if (VMultiDeviceDetector.TryOpenOutputDevice(out var virtualHidDevice, out var status))
            {
                extended = status.IsExtendedDigitizerAvailable;
                return virtualHidDevice;
            }

            extended = false;
            Log.WriteNotify(name, status.Message, LogLevel.Error);
            return null;
        }
    }

    internal class VMultiInstance<T> : VMultiInstance where T : unmanaged
    {
        public unsafe T* Pointer { get; }

        public unsafe VMultiInstance(string name, Func<bool, T> initialValue) : base(name, Unsafe.SizeOf<T>())
        {
            Pointer = (T*)Unsafe.AsPointer(ref Buffer[0]);
            *Pointer = initialValue(Extended);
        }
    }
}
