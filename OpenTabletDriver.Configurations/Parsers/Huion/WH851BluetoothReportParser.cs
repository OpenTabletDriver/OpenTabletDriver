using System.Numerics;
using System.Runtime.CompilerServices;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Configurations.Parsers.Huion
{
    public class WH851BluetoothReportParser : IReportParser<IDeviceReport>
    {
        private readonly InspiroyReportParser _inspiroyReportParser = new();

        public IDeviceReport Parse(byte[] data)
        {
            if (data.Length >= 12 && data[0] == 0x08)
                return _inspiroyReportParser.Parse(data);

            if (data.Length >= 10 && data[0] == 0x0a)
            {
                if (!data[1].IsBitSet(6))
                    return new OutOfRangeReport(data);

                return new WH851BluetoothPenReport(data);
            }

            return new DeviceReport(data);
        }
    }

    public struct WH851BluetoothPenReport : ITabletReport, ITiltReport, IEraserReport
    {
        public WH851BluetoothPenReport(byte[] report)
        {
            Raw = report;
            Position = new Vector2
            {
                X = Unsafe.ReadUnaligned<ushort>(ref report[2]),
                Y = Unsafe.ReadUnaligned<ushort>(ref report[4])
            };
            Pressure = Unsafe.ReadUnaligned<ushort>(ref report[6]);
            Tilt = new Vector2
            {
                X = (sbyte)report[8],
                Y = (sbyte)report[9]
            };

            PenButtons = new[]
            {
                report[1].IsBitSet(1),
                report[1].IsBitSet(2)
            };
            Eraser = report[1].IsBitSet(2);
        }

        public byte[] Raw { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Tilt { get; set; }
        public uint Pressure { get; set; }
        public bool[] PenButtons { get; set; }
        public bool Eraser { get; set; }
    }
}
