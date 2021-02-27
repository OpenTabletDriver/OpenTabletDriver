using System;
using System.Text;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.UX.Tools
{
    public static class ReportFormatter
    {
        public static string GetStringRaw(IDeviceReport report)
        {
            return BitConverter.ToString(report.Raw).Replace('-', ' ');
        }

        public static string GetStringFormat(IDeviceReport report)
        {
            StringBuilder sb = new StringBuilder();

            if (report is ITabletReport tabletReport)
                sb.AppendLine(GetStringFormat(tabletReport));
            if (report is IAuxReport auxReport)
                sb.AppendLine(GetStringFormat(auxReport));
            if (report is IEraserReport eraserReport)
                sb.AppendLine(GetStringFormat(eraserReport));
            if (report is IProximityReport proximityReport)
                sb.AppendLine(GetStringFormat(proximityReport));
            if (report is ITiltReport tiltReport)
                sb.AppendLine(GetStringFormat(tiltReport));

            return sb.ToString();
        }

        private static string GetStringFormat(ITabletReport tabletReport)
        {
            return $"ReportID:{tabletReport.ReportID}, " +
                $"Position:[{tabletReport.Position.X},{tabletReport.Position.Y}], " +
                $"Pressure:{tabletReport.Pressure}, " +
                $"PenButtons:[{string.Join(" ", tabletReport.PenButtons)}]";
        }

        private static string GetStringFormat(IAuxReport auxReport)
        {
            return $"AuxButtons:[{string.Join(" ", auxReport.AuxButtons)}]";
        }

        private static string GetStringFormat(IEraserReport eraserReport)
        {
            return $"Eraser:{eraserReport.Eraser}";
        }

        private static string GetStringFormat(IProximityReport proximityReport)
        {
            return $"NearProximity:{proximityReport.NearProximity}, " +
                $"HoverDistance:{proximityReport.HoverDistance}";
        }

        private static string GetStringFormat(ITiltReport tiltReport)
        {
            return $"Tilt:[{tiltReport.Tilt}]";
        }
    }
}
