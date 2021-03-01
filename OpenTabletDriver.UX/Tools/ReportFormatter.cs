using System;
using System.Collections.Generic;
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
                sb.AppendLines(GetStringFormat(tabletReport));
            if (report is IAuxReport auxReport)
                sb.AppendLines(GetStringFormat(auxReport));
            if (report is IEraserReport eraserReport)
                sb.AppendLines(GetStringFormat(eraserReport));
            if (report is IProximityReport proximityReport)
                sb.AppendLines(GetStringFormat(proximityReport));
            if (report is ITiltReport tiltReport)
                sb.AppendLines(GetStringFormat(tiltReport));

            return sb.ToString();
        }

        private static IEnumerable<string> GetStringFormat(ITabletReport tabletReport)
        {
            yield return $"ReportID:{tabletReport.ReportID}";
            yield return $"Position:[{tabletReport.Position.X},{tabletReport.Position.Y}]";
            yield return $"Pressure:{tabletReport.Pressure}";
            yield return $"PenButtons:[{string.Join(" ", tabletReport.PenButtons)}]";
        }

        private static IEnumerable<string> GetStringFormat(IAuxReport auxReport)
        {
            yield return $"AuxButtons:[{string.Join(" ", auxReport.AuxButtons)}]";
        }

        private static IEnumerable<string> GetStringFormat(IEraserReport eraserReport)
        {
            yield return $"Eraser:{eraserReport.Eraser}";
        }

        private static IEnumerable<string> GetStringFormat(IProximityReport proximityReport)
        {
            yield return $"NearProximity:{proximityReport.NearProximity}";
            yield return $"HoverDistance:{proximityReport.HoverDistance}";
        }

        private static IEnumerable<string> GetStringFormat(ITiltReport tiltReport)
        {
            yield return $"Tilt:[{tiltReport.Tilt.X},{tiltReport.Tilt.Y}]";
        }

        private static void AppendLines(this StringBuilder sb, IEnumerable<string> lines)
        {
            foreach (var line in lines)
                sb.AppendLine(line);
        }
    }
}
