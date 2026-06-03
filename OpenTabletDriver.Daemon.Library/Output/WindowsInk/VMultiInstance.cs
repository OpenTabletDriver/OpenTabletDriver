using System;
using System.Linq;
using System.Runtime.CompilerServices;
using HidSharp;
using HidSharp.Reports;
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
            HidStream? virtualHidDevice = null;
            var devices = DeviceList.Local.GetHidDevices(vendorID: 0x00ff, productID: 0xbacc).ToArray();

            foreach (var device in devices)
            {
                if (device.GetMaxOutputReportLength() == 65 && device.GetMaxInputReportLength() == 65)
                {
                    if (device.TryOpen(out virtualHidDevice))
                        break;
                }
            }

            var normal = false;
            extended = false;

            foreach (var device in devices)
            {
                if (device.GetMaxInputReportLength() != 10)
                    continue;

                var reportDescriptor = device.GetReportDescriptor();
                if (reportDescriptor.TryGetReport(ReportType.Input, DigitizerInputReport.NormalReportId, out _))
                    normal = true;
                if (reportDescriptor.TryGetReport(ReportType.Input, DigitizerInputReport.ExtendedReportId, out _))
                    extended = true;

                if (normal && extended)
                    break;
            }

            if (virtualHidDevice == null || (!normal && !extended))
            {
                Log.WriteNotify(
                    name,
                    "Cannot find VMulti VirtualHID. Install VMulti driver, then restart OpenTabletDriver.",
                    LogLevel.Error
                );
            }

            return virtualHidDevice;
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
