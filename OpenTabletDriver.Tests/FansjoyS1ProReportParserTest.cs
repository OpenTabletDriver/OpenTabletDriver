using System;
using System.Buffers.Binary;
using System.Linq;
using System.Numerics;
using OpenTabletDriver.Configurations.Parsers.XENX;
using OpenTabletDriver.Plugin.Tablet;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class FansjoyS1ProReportParserTest
    {
        [Theory]
        [InlineData(0x01)]
        [InlineData(0x0a)]
        public void PenReport_DecodesAllFields(byte reportId)
        {
            var data = new byte[64];
            data[0] = reportId;
            data[1] = 0x67;
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(2), 21000);
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(4), 13000);
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(6), 8191);
            BinaryPrimitives.WriteInt16LittleEndian(data.AsSpan(8), -4500);
            BinaryPrimitives.WriteInt16LittleEndian(data.AsSpan(10), 6000);

            var report = Assert.IsType<FansjoyS1ProTabletReport>(new FansjoyS1ProReportParser().Parse(data));

            Assert.Equal(new Vector2(21000, 13000), report.Position);
            Assert.Equal((uint)8191, report.Pressure);
            Assert.Equal(new Vector2(-45, 60), report.Tilt);
            Assert.Equal([true, true], report.PenButtons);
            Assert.True(report.Eraser);
        }

        [Fact]
        public void ZeroState_ReturnsOutOfRangeReport()
        {
            var data = new byte[64];
            data[0] = 0x01;

            Assert.IsType<OutOfRangeReport>(new FansjoyS1ProReportParser().Parse(data));
        }

        [Fact]
        public void AuxReport_DecodesOnlyEighteenTabletButtons()
        {
            var data = new byte[64];
            data[0] = 0x02;
            data[2] = 1;
            data[19] = 1;
            data[20] = 1;
            data[21] = 1;

            var report = Assert.IsType<FansjoyS1ProAuxReport>(new FansjoyS1ProReportParser().Parse(data));

            Assert.Equal(18, report.AuxButtons.Length);
            Assert.True(report.AuxButtons[0]);
            Assert.True(report.AuxButtons[17]);
            Assert.All(report.AuxButtons.Skip(1).Take(16), Assert.False);
        }

        [Fact]
        public void UnknownReport_ReturnsDeviceReport()
        {
            var data = new byte[64];
            data[0] = 0xff;

            Assert.IsType<DeviceReport>(new FansjoyS1ProReportParser().Parse(data));
        }
    }
}
