using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HidSharp.Reports;
using OpenTabletDriver.Interop;

namespace OpenTabletDriver.Devices.HidSharpBackend
{
    internal static class Extensions
    {
        // - HID_REPORTS (report_id:usage_page:usage_id, ...)
        public static void ExtractHidUsages(Dictionary<string, string> deviceAttributes, Func<ReportDescriptor> reportDescriptorFunc)
        {
            try
            {
                var reportDescriptor = reportDescriptorFunc();
                var usages = new List<(byte, uint)>();
                foreach (var inputReport in reportDescriptor.InputReports)
                {
                    var reportId = inputReport.ReportID;
                    usages.AddRange(inputReport.DeviceItem.Usages.GetAllValues().Select(x => (reportId, x)));
                }

                var hidReportsBuilder = new StringBuilder();
                var enumerator = usages.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    var reportId = enumerator.Current.Item1;
                    var extendedUsage = enumerator.Current.Item2;
                    appendHidReport(hidReportsBuilder, reportId, extendedUsage);
                    while (enumerator.MoveNext())
                    {
                        hidReportsBuilder.Append(", ");
                        reportId = enumerator.Current.Item1;
                        extendedUsage = enumerator.Current.Item2;
                        appendHidReport(hidReportsBuilder, reportId, extendedUsage);
                    }

                    static void appendHidReport(StringBuilder stringBuilder, byte reportId, uint extendedUsage)
                    {
                        var usagePage = (extendedUsage & 0xffff0000) >> 16;
                        var usageId = extendedUsage & 0x0000ffff;
                        stringBuilder.Append($"{reportId:X2}:{usagePage:X4}:{usageId:X4}");
                    }
                }

                deviceAttributes.Add("HID_REPORTS", hidReportsBuilder.ToString());
            }
            catch
            {
                deviceAttributes.Add("HID_REPORTS_NON_RECONSTRUCTABLE", "true");
            }
        }
    }
}
