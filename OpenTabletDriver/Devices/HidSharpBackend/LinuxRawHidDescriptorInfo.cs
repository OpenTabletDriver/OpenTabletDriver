using System;
using System.Collections.Generic;
using System.IO;

namespace OpenTabletDriver.Devices.HidSharpBackend
{
    /// <summary>
    /// Report-shape metadata extracted from hidraw without constructing HidSharp's
    /// <see cref="HidSharp.Reports.ReportDescriptor"/>. This is intentionally a
    /// tolerant parser: it only reconstructs the parts needed for matching and raw
    /// streaming, so malformed items such as the signed-byte UnitExponent variant
    /// do not block the fallback.
    /// </summary>
    internal sealed class LinuxRawHidDescriptorInfo
    {
        internal LinuxRawHidDescriptorInfo(int inputReportLength, int outputReportLength, int featureReportLength, bool reportsUseID)
        {
            InputReportLength = inputReportLength;
            OutputReportLength = outputReportLength;
            FeatureReportLength = featureReportLength;
            ReportsUseID = reportsUseID;
        }

        public int InputReportLength { get; }
        public int OutputReportLength { get; }
        public int FeatureReportLength { get; }
        public bool ReportsUseID { get; }

        /// <summary>
        /// Reads the raw hidraw descriptor directly from the kernel and extracts
        /// only the report lengths and report-ID usage required by the fallback.
        /// </summary>
        internal static LinuxRawHidDescriptorInfo TryCreateFromDevice(string devicePath)
        {
            if (string.IsNullOrWhiteSpace(devicePath))
                return null;

            try
            {
                using var stream = new FileStream(devicePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                var fd = stream.SafeFileHandle.DangerousGetHandle().ToInt32();

                if (LinuxHidrawInterop.ioctl(fd, LinuxHidrawInterop.HIDIOCGRDESCSIZE, out var size) < 0
                    || size <= 0
                    || size > LinuxHidrawInterop.HidMaxDescriptorSize)
                {
                    return null;
                }

                var rawDescriptor = new LinuxHidrawInterop.HidrawReportDescriptor()
                {
                    Size = (uint)size,
                    Value = new byte[LinuxHidrawInterop.HidMaxDescriptorSize]
                };

                if (LinuxHidrawInterop.ioctl(fd, LinuxHidrawInterop.HIDIOCGRDESC, ref rawDescriptor) < 0)
                    return null;

                Array.Resize(ref rawDescriptor.Value, size);
                return TryParse(rawDescriptor.Value, out var info) ? info : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parses enough of the HID descriptor to recover the maximum report
        /// lengths. Global items that affect report sizing are honored; metadata
        /// items that do not affect layout are ignored.
        /// </summary>
        internal static bool TryParse(byte[] descriptor, out LinuxRawHidDescriptorInfo info)
        {
            info = null;
            if (descriptor == null)
                return false;

            var reports = new Dictionary<int, ReportBitLengths>();
            var global = new GlobalState();
            var stack = new Stack<GlobalState>();
            bool reportsUseID = false;

            for (int index = 0; index < descriptor.Length;)
            {
                var prefix = descriptor[index++];
                if (prefix == 0xFE)
                {
                    if (index + 1 >= descriptor.Length)
                        return false;

                    var dataSize = descriptor[index++];
                    index++; // long item tag

                    if (index + dataSize > descriptor.Length)
                        return false;

                    index += dataSize;
                    continue;
                }

                var size = GetItemSize(prefix & 0x03);
                if (index + size > descriptor.Length)
                    return false;

                if (!TryReadUInt32(descriptor, index, size, out var value))
                    return false;

                index += size;

                var type = (prefix >> 2) & 0x03;
                var tag = (prefix >> 4) & 0x0F;

                switch (type)
                {
                    case 0x00:
                        if (tag == 0x08 || tag == 0x09 || tag == 0x0B)
                        {
                            if (!TryAccumulateBits(reports, global, tag))
                                return false;
                        }
                        break;

                    case 0x01:
                        switch (tag)
                        {
                            case 0x07:
                                if (value > int.MaxValue)
                                    return false;

                                global.ReportSizeBits = (int)value;
                                break;

                            case 0x08:
                                // The public stream contract expects a leading report-ID slot
                                // even when the kernel delivers unnumbered reports directly.
                                if (value == 0 || value > int.MaxValue)
                                    return false;

                                reportsUseID = true;
                                global.ReportID = (int)value;
                                GetOrCreateReport(reports, global.ReportID);
                                break;

                            case 0x09:
                                if (value > int.MaxValue)
                                    return false;

                                global.ReportCount = (int)value;
                                break;

                            case 0x0A:
                                stack.Push(global);
                                break;

                            case 0x0B:
                                if (stack.Count == 0)
                                    return false;

                                global = stack.Pop();
                                break;
                        }
                        break;
                }
            }

            int maxInputBits = 0;
            int maxOutputBits = 0;
            int maxFeatureBits = 0;

            foreach (var report in reports.Values)
            {
                maxInputBits = Math.Max(maxInputBits, report.InputBits);
                maxOutputBits = Math.Max(maxOutputBits, report.OutputBits);
                maxFeatureBits = Math.Max(maxFeatureBits, report.FeatureBits);
            }

            info = new LinuxRawHidDescriptorInfo(
                ToReportLength(maxInputBits),
                ToReportLength(maxOutputBits),
                ToReportLength(maxFeatureBits),
                reportsUseID);

            return true;
        }

        private static int GetItemSize(int sizeCode) => sizeCode == 0x03 ? 4 : sizeCode;

        private static bool TryReadUInt32(byte[] buffer, int offset, int count, out uint value)
        {
            value = 0;

            if (count < 0 || count > sizeof(uint))
                return false;

            for (int index = 0; index < count; index++)
                value |= (uint)buffer[offset + index] << (index * 8);

            return true;
        }

        private static bool TryAccumulateBits(Dictionary<int, ReportBitLengths> reports, GlobalState global, int tag)
        {
            if (global.ReportSizeBits < 0 || global.ReportCount < 0)
                return false;

            int bits;
            try
            {
                bits = checked(global.ReportSizeBits * global.ReportCount);
            }
            catch (OverflowException)
            {
                return false;
            }

            var report = GetOrCreateReport(reports, global.ReportID);

            try
            {
                checked
                {
                    switch (tag)
                    {
                        case 0x08:
                            report.InputBits += bits;
                            break;

                        case 0x09:
                            report.OutputBits += bits;
                            break;

                        case 0x0B:
                            report.FeatureBits += bits;
                            break;
                    }
                }
            }
            catch (OverflowException)
            {
                return false;
            }

            return true;
        }

        private static ReportBitLengths GetOrCreateReport(Dictionary<int, ReportBitLengths> reports, int reportID)
        {
            if (!reports.TryGetValue(reportID, out var report))
            {
                report = new ReportBitLengths();
                reports.Add(reportID, report);
            }

            return report;
        }

        private static int ToReportLength(int bits)
        {
            if (bits == 0)
                return 0;

            // Match HidSharp's public contract: lengths include the report ID byte.
            return ((bits + 7) / 8) + 1;
        }

        private struct GlobalState
        {
            public int ReportSizeBits;
            public int ReportCount;
            public int ReportID;
        }

        private sealed class ReportBitLengths
        {
            public int InputBits { get; set; }
            public int OutputBits { get; set; }
            public int FeatureBits { get; set; }
        }
    }
}
