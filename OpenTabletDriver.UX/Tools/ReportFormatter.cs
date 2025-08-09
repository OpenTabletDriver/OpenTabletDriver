using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Touch;

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
            var sb = new StringBuilder();

            if (report is IAbsolutePositionReport absolutePositionReport)
                sb.AppendLines(GetStringFormat(absolutePositionReport));
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
            if (report is ITouchReport touchReport)
                sb.AppendLines(GetStringFormat(touchReport));
            if (report is IMouseReport mouseReport)
                sb.AppendLines(GetStringFormat(mouseReport));
            if (report is IToolReport toolReport)
                sb.AppendLines(GetStringFormat(toolReport));
            if (report is OutOfRangeReport oorReport)
                sb.AppendLines(GetStringFormat(oorReport));

            return sb.ToString();
        }

        public static string GetStringFormatOneLine(IDeviceReport report, TimeSpan delta, string reportType)
        {
            var sb = new StringBuilder();

            sb.Append($"{{ {GetStringRaw(report)} }}");
            sb.Append($", Delta:{delta.TotalMilliseconds:F3}ms");
            if (report is IAbsolutePositionReport absolutePositionReport)
                sb.AppendOneLine(GetStringFormat(absolutePositionReport));
            if (report is ITabletReport tabletReport)
                sb.AppendOneLine(GetStringFormat(tabletReport));
            if (report is IAuxReport auxReport)
                sb.AppendOneLine(GetStringFormat(auxReport));
            if (report is IEraserReport eraserReport)
                sb.AppendOneLine(GetStringFormat(eraserReport));
            if (report is IProximityReport proximityReport)
                sb.AppendOneLine(GetStringFormat(proximityReport));
            if (report is ITiltReport tiltReport)
                sb.AppendOneLine(GetStringFormat(tiltReport));
            if (report is ITouchReport touchReport)
                sb.AppendOneLine(GetStringFormat(touchReport));
            if (report is IMouseReport mouseReport)
                sb.AppendOneLine(GetStringFormat(mouseReport));
            if (report is IToolReport toolReport)
                sb.AppendOneLine(GetStringFormat(toolReport));
            if (report is OutOfRangeReport oorReport)
                sb.AppendOneLine(GetStringFormat(oorReport));

            reportType = reportType.StartsWith("OpenTabletDriver.Configurations.Parsers")
                ? reportType.Split('.').Last()
                : reportType;
            sb.Append(", ReportType:" + reportType);

            return sb.ToString();
        }

        private static IEnumerable<string> GetStringFormat(IAbsolutePositionReport absolutePositionReport)
        {
            yield return $"Position:[{absolutePositionReport.Position.X},{absolutePositionReport.Position.Y}]";
        }

        private static IEnumerable<string> GetStringFormat(ITabletReport tabletReport)
        {
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

        private static IEnumerable<string> GetStringFormat(ITouchReport touchReport)
        {
            yield return $"Touch data:";
            foreach (var touch in touchReport.Touches)
                if (touch != null)
                    yield return touch.ToString();
        }

        private static IEnumerable<string> GetStringFormat(IMouseReport mouseReport)
        {
            yield return $"MouseButtons:[{string.Join(" ", mouseReport.MouseButtons)}]";
            yield return $"Scroll:[{mouseReport.Scroll.X},{mouseReport.Scroll.Y}]";
        }

        private static IEnumerable<string> GetStringFormat(IToolReport toolReport)
        {
            yield return $"Tool:{Enum.GetName(toolReport.Tool)}";
            yield return $"RawToolID:{toolReport.RawToolID}";
            yield return $"Serial:{toolReport.Serial}";
        }

        private static IEnumerable<string> GetStringFormat(OutOfRangeReport oorReport)
        {
            yield return $"Pen is out of Range";
        }

        private static void AppendLines(this StringBuilder sb, IEnumerable<string> lines)
        {
            foreach (var line in lines)
                sb.AppendLine(line);
        }

        private static void AppendOneLine(this StringBuilder sb, IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                sb.Append(", ");
                sb.Append(line);
            }
        }
    }
}
