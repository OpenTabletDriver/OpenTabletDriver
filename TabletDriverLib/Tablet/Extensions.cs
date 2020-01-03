using System;
using System.Linq;

namespace TabletDriverLib.Tablet
{
    internal static class Extensions
    {
        public static IDeviceReportParser GetReportParser(this TabletProperties tablet)
        {
            return GetReportParser(tablet.ReportParserName);
        }

        public static IDeviceReportParser GetCustomReportParser(this TabletProperties tablet)
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

        public static string GetData(this ITabletReport report, bool raw)
        {
            if (raw)
                return BitConverter.ToString(report.Raw).Replace('-', ' ');
            else
                return $"Lift:{report.Lift}, Position:[{report.Position}], Pressure:{report.Pressure}, PenButtons:[{String.Join(" ", report.PenButtons)}]";
        }
    }
}