using System;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Tablet
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
                return report switch
                {
                    IAuxReport auxReport =>
                        $"AuxButtons:[{String.Join(" ", auxReport.AuxButtons)}]",
                    ITabletReport tabletReport =>
                        $"ReportID:{tabletReport.ReportID}, " + 
                        $"Position:[{tabletReport.Position.X},{tabletReport.Position.Y}], " + 
                        $"Pressure:{tabletReport.Pressure}, " + 
                        $"PenButtons:[{String.Join(" ", tabletReport.PenButtons)}]",
                    _ =>
                        $"Raw: {BitConverter.ToString(report.Raw).Replace('-', ' ')}"
                };
            }
        }
    }
}