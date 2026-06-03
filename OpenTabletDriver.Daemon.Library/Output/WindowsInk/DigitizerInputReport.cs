using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Daemon.Library.Output.WindowsInk
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct VMultiReportHeader
    {
        public VMultiReportHeader(int size, byte reportId)
        {
            VMultiId = 0x40;
            ReportLength = (byte)(size - 1);
            ReportId = reportId;
            Buttons = 0;
        }

        public byte VMultiId;
        public byte ReportLength;
        public byte ReportId;
        public byte Buttons;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct DigitizerInputReport
    {
        public const byte NormalReportId = 0x05;
        public const byte ExtendedReportId = 0x06;

        private DigitizerInputReport(byte reportId)
        {
            Header = new VMultiReportHeader(Unsafe.SizeOf<DigitizerInputReport>(), reportId);
            X = 0;
            Y = 0;
            Pressure = 0;
            XTilt = 0;
            YTilt = 0;
        }

        public static DigitizerInputReport Normal() => new(NormalReportId);
        public static DigitizerInputReport Extended() => new(ExtendedReportId);

        public VMultiReportHeader Header;
        public ushort X;
        public ushort Y;
        public ushort Pressure;
        public byte XTilt;
        public byte YTilt;
    }
}
