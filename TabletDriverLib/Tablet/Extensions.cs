using System;
using System.Collections.Generic;
using System.Linq;
using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Tablet
{
    public static class Extensions
    {
        internal static IDeviceReportParser GetReportParser(this TabletProperties tablet)
        {
            return GetReportParser(tablet.ReportParserName);
        }

        internal static IDeviceReportParser GetCustomReportParser(this TabletProperties tablet)
        {
            return GetReportParser(tablet.CustomReportParserName);
        }

        private static IDeviceReportParser GetReportParser(string name)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetTypes().FirstOrDefault(t => t.FullName == name);
                if (type != null)
                {
                    var ctorResult = type.GetConstructors().FirstOrDefault().Invoke(new object[]{});
                    return ctorResult as IDeviceReportParser;
                }
            }
            return null;
        }

        public static string StringFormat(this IDeviceReport report, bool raw)
        {
            if (raw)
            {
                return BitConverter.ToString(report.Raw).Replace('-', ' ');
            }
            else
            {
                if (report is IAuxReport auxReport)
                {
                    return $"AuxButtons:[{String.Join(" ", auxReport.AuxButtons)}]";
                }
                else if (report is ITabletReport tabletReport)
                {
                    return $"Lift:{tabletReport.Lift}, Position:[{tabletReport.Position}], Pressure:{tabletReport.Pressure}, PenButtons:[{String.Join(" ", tabletReport.PenButtons)}]";
                }
                else
                {
                    return report.ToString();
                }
            }
        }
    }
}