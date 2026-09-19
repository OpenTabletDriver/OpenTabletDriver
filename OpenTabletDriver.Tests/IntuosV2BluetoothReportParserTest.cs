using Newtonsoft.Json.Linq;
using System.Numerics;
using OpenTabletDriver.Configurations.Parsers.Wacom.IntuosV2;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Plugin.Tablet;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class IntuosV2BluetoothReportParserTest
    {
        [Fact]
        public void Parse_Uses_Last_Valid_Pen_Frame()
        {
            var parser = new IntuosV2BluetoothReportParser();
            var data = CreateReport(0xE0, 100, 200, 0, 10);
            WritePenFrame(data, 9, 0xE4, 300, 400, 1234, 20);

            var report = Assert.IsType<IntuosV2BluetoothReport>(parser.Parse(data));

            Assert.Equal(new Vector2(300, 400), report.Position);
            Assert.Equal(1234u, report.Pressure);
            Assert.Equal(new[] { false, true }, report.PenButtons);
            Assert.True(report.NearProximity);
            Assert.Equal(20u, report.HoverDistance);
        }

        [Fact]
        public void Parse_Reports_Auxiliary_Buttons_Without_Pen_Data()
        {
            var parser = new IntuosV2BluetoothReportParser();
            var data = CreateReport();
            data[44] = 0b0000_0101;

            var report = parser.Parse(data);
            var auxReport = Assert.IsAssignableFrom<IAuxReport>(report);

            Assert.IsNotAssignableFrom<ITabletReport>(report);
            Assert.Equal(new[] { true, false, true, false }, auxReport.AuxButtons);
        }

        [Fact]
        public void Parse_Auxiliary_Report_Can_Be_Read_By_Tablet_Debugger()
        {
            var parser = new IntuosV2BluetoothReportParser();
            var data = CreateReport();
            data[44] = 0b0000_0101;
            var report = parser.Parse(data);
            var serializedReport = new DebugReportData
            {
                Tablet = null!,
                Path = report.GetType().FullName!,
                Data = JToken.FromObject(report)
            };

            var deserializedReport = Assert.IsAssignableFrom<IAuxReport>(serializedReport.ToObject());

            Assert.Equal(new[] { true, false, true, false }, deserializedReport.AuxButtons);
        }

        [Fact]
        public void Parse_Preserves_Position_When_Pen_Is_Outside_Range()
        {
            var parser = new IntuosV2BluetoothReportParser();
            parser.Parse(CreateReport(0xE0, 100, 200, 0, 10));

            var report = Assert.IsType<IntuosV2BluetoothReport>(
                parser.Parse(CreateReport(0xC0, 500, 600, 0, 63)));

            Assert.Equal(new Vector2(100, 200), report.Position);
            Assert.True(report.NearProximity);
            Assert.Equal(63u, report.HoverDistance);
        }

        [Fact]
        public void Parse_Emits_Out_Of_Range_Before_Auxiliary_Only_Reports()
        {
            var parser = new IntuosV2BluetoothReportParser();

            Assert.IsType<OutOfRangeReport>(parser.Parse(CreateReport(0x80)));

            var data = CreateReport();
            data[44] = 0b0000_0010;
            var auxReport = Assert.IsAssignableFrom<IAuxReport>(parser.Parse(data));
            Assert.Equal(new[] { false, true, false, false }, auxReport.AuxButtons);
        }

        [Fact]
        public void Parse_Rejects_Incomplete_Bluetooth_Reports()
        {
            var parser = new IntuosV2BluetoothReportParser();
            var shortReport = new byte[45];
            shortReport[0] = 0x81;

            Assert.IsType<DeviceReport>(parser.Parse([]));
            Assert.IsType<DeviceReport>(parser.Parse(shortReport));
        }

        private static byte[] CreateReport(
            byte status = 0,
            ushort x = 0,
            ushort y = 0,
            ushort pressure = 0,
            byte hoverDistance = 0)
        {
            var data = new byte[46];
            data[0] = 0x81;
            WritePenFrame(data, 1, status, x, y, pressure, hoverDistance);
            return data;
        }

        private static void WritePenFrame(
            byte[] data,
            int offset,
            byte status,
            ushort x,
            ushort y,
            ushort pressure,
            byte hoverDistance)
        {
            data[offset] = status;
            data[offset + 1] = (byte)x;
            data[offset + 2] = (byte)(x >> 8);
            data[offset + 3] = (byte)y;
            data[offset + 4] = (byte)(y >> 8);
            data[offset + 5] = (byte)pressure;
            data[offset + 6] = (byte)(pressure >> 8);
            data[offset + 7] = hoverDistance;
        }
    }
}
