using OpenTabletDriver.Configurations.Parsers.Huion;
using OpenTabletDriver.Tablet;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public sealed class WH851ReportParserTests
    {
        [Fact]
        public void BluetoothParserParsesStandardBlePenReport()
        {
            var parser = new WH851BluetoothReportParser();
            var report = parser.Parse(new byte[]
            {
                0x0a,
                0x40 | 0x02,
                0x34, 0x12,
                0x78, 0x56,
                0xbc, 0x0a,
                0xfe,
                0x05
            });

            var tabletReport = Assert.IsType<WH851BluetoothPenReport>(report);
            Assert.Equal(0x1234, tabletReport.Position.X);
            Assert.Equal(0x5678, tabletReport.Position.Y);
            Assert.Equal(0x0abcU, tabletReport.Pressure);
            Assert.Equal(-2, tabletReport.Tilt.X);
            Assert.Equal(5, tabletReport.Tilt.Y);
            Assert.True(tabletReport.PenButtons[0]);
            Assert.False(tabletReport.PenButtons[1]);
            Assert.False(tabletReport.Eraser);
        }

        [Fact]
        public void BluetoothParserReturnsOutOfRangeWhenBleInRangeBitIsClear()
        {
            var parser = new WH851BluetoothReportParser();

            var report = parser.Parse(new byte[]
            {
                0x0a,
                0x00,
                0x34, 0x12,
                0x78, 0x56,
                0xbc, 0x0a,
                0xfe,
                0x05
            });

            Assert.IsType<OutOfRangeReport>(report);
        }

        [Fact]
        public void BluetoothParserFallsBackToInspiroyParserForUsbStyleTiltReport()
        {
            var parser = new WH851BluetoothReportParser();

            var report = parser.Parse(new byte[]
            {
                0x08,
                0x80 | 0x02,
                0x34, 0x12,
                0x78, 0x56,
                0xbc, 0x0a,
                0x00, 0x00,
                0xfe,
                0x05
            });

            var tabletReport = Assert.IsType<TiltTabletReport>(report);
            Assert.Equal(0x1234, tabletReport.Position.X);
            Assert.Equal(0x5678, tabletReport.Position.Y);
            Assert.Equal(0x0abcU, tabletReport.Pressure);
            Assert.Equal(-2, tabletReport.Tilt.X);
            Assert.Equal(5, tabletReport.Tilt.Y);
            Assert.True(tabletReport.PenButtons[0]);
        }

        [Fact]
        public void BluetoothParserPreservesUnknownReports()
        {
            var parser = new WH851BluetoothReportParser();
            var raw = new byte[] { 0x99, 0x01, 0x02 };

            var report = parser.Parse(raw);

            var deviceReport = Assert.IsType<DeviceReport>(report);
            Assert.Same(raw, deviceReport.Raw);
        }

    }
}

