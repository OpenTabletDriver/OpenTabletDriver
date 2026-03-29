using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using OpenTabletDriver.Devices.HidSharpBackend;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class LinuxRawHidStreamTest
    {
        [Fact]
        public void Read_DoesNotTruncateLargeReports()
        {
            var path = CreateTempFile(Enumerable.Range(0, 192).Select(i => (byte)i).ToArray());
            try
            {
                var info = new LinuxRawHidDescriptorInfo(192, 0, 0, true);
                using var stream = new LinuxRawHidStream(path, info);

                var report = stream.Read();

                Assert.Equal(192, report.Length);
                Assert.Equal(Enumerable.Range(0, 192).Select(i => (byte)i), report);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void Read_PrependsSyntheticReportId_WhenDescriptorHasNoReportIds()
        {
            var path = CreateTempFile(new byte[] { 0x10, 0x20, 0x30 });
            try
            {
                var info = new LinuxRawHidDescriptorInfo(4, 0, 0, false);
                using var stream = new LinuxRawHidStream(path, info);

                var report = stream.Read();

                Assert.Equal(new byte[] { 0x00, 0x10, 0x20, 0x30 }, report);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void GetFeature_PrependsSyntheticReportId_WhenDescriptorHasNoReportIds()
        {
            var path = CreateTempFile(Array.Empty<byte>());
            try
            {
                var info = new LinuxRawHidDescriptorInfo(0, 0, 4, false);
                using var stream = new LinuxRawHidStream(path, info, (_, _, data) =>
                {
                    Marshal.Copy(new byte[] { 0xAB, 0xCD }, 0, data, 2);
                    return 2;
                });

                var buffer = new byte[4];
                stream.GetFeature(buffer);

                Assert.Equal(new byte[] { 0x00, 0xAB, 0xCD, 0x00 }, buffer);
            }
            finally
            {
                File.Delete(path);
            }
        }

        private static string CreateTempFile(byte[] contents)
        {
            var path = Path.GetTempFileName();
            File.WriteAllBytes(path, contents);
            return path;
        }
    }
}
