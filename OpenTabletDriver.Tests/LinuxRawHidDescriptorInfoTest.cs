using OpenTabletDriver.Devices.HidSharpBackend;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class LinuxRawHidDescriptorInfoTest
    {
        [Fact]
        public void TryParse_AddsSyntheticReportIdSlot_WhenDescriptorHasNoReportIds()
        {
            var descriptor = new byte[]
            {
                0x75, 0x08,
                0x95, 0x03,
                0x81, 0x02,
                0x95, 0x02,
                0x91, 0x02,
                0x95, 0x04,
                0xB1, 0x02
            };

            Assert.True(LinuxRawHidDescriptorInfo.TryParse(descriptor, out var info));
            Assert.False(info.ReportsUseID);
            Assert.Equal(4, info.InputReportLength);
            Assert.Equal(3, info.OutputReportLength);
            Assert.Equal(5, info.FeatureReportLength);
        }

        [Fact]
        public void TryParse_UsesLargestReportAcrossReportIds()
        {
            var descriptor = new byte[]
            {
                0x85, 0x01,
                0x75, 0x08,
                0x95, 0x03,
                0x81, 0x02,
                0x95, 0x02,
                0x91, 0x02,
                0x85, 0x02,
                0x95, 0x05,
                0x81, 0x02,
                0x95, 0x04,
                0xB1, 0x02
            };

            Assert.True(LinuxRawHidDescriptorInfo.TryParse(descriptor, out var info));
            Assert.True(info.ReportsUseID);
            Assert.Equal(6, info.InputReportLength);
            Assert.Equal(3, info.OutputReportLength);
            Assert.Equal(5, info.FeatureReportLength);
        }

        [Fact]
        public void TryParse_IgnoresMalformedUnitExponent()
        {
            var descriptor = new byte[]
            {
                0x85, 0x01,
                0x75, 0x08,
                0x95, 0x02,
                0x55, 0xFD,
                0x81, 0x02
            };

            Assert.True(LinuxRawHidDescriptorInfo.TryParse(descriptor, out var info));
            Assert.True(info.ReportsUseID);
            Assert.Equal(3, info.InputReportLength);
        }

        [Fact]
        public void TryParse_RestoresGlobalStateAcrossPushAndPop()
        {
            var descriptor = new byte[]
            {
                0x85, 0x01,
                0x75, 0x08,
                0x95, 0x01,
                0xA4,
                0x75, 0x10,
                0x95, 0x02,
                0x81, 0x02,
                0xB4,
                0xB1, 0x02
            };

            Assert.True(LinuxRawHidDescriptorInfo.TryParse(descriptor, out var info));
            Assert.Equal(5, info.InputReportLength);
            Assert.Equal(2, info.FeatureReportLength);
        }
    }
}
