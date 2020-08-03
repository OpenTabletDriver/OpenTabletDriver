using System;
using System.Collections.Generic;
using System.Linq;
using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Tablet
{
    public static class Extensions
    {
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
                    return $"ReportID:{tabletReport.ReportID}, Position:[{tabletReport.Position}], Pressure:{tabletReport.Pressure}, PenButtons:[{String.Join(" ", tabletReport.PenButtons)}]";
                }
                else
                {
                    return $"Raw: {BitConverter.ToString(report.Raw).Replace('-', ' ')}";
                }
            }
        }
    }
}