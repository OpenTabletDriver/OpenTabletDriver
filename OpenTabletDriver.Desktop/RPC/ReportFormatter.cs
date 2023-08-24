using System;
using System.Collections.Generic;
using System.Text;
using OpenTabletDriver.Tablet;
using OpenTabletDriver.Tablet.Touch;

namespace OpenTabletDriver.Desktop.RPC
{
    public static class ReportFormatter
    {
        public static string GetStringRaw(byte[] raw)
        {
            var sb = new StringBuilder();

            var column = 0;
            foreach (var b in raw)
            {
                column++;
                if (column > 8)
                {
                    column = 0;
                    sb.AppendLine();
                }

                sb.Append(b.ToString("X2"));
                sb.Append(' ');
            }

            return sb.ToString();
        }

        public static string GetStringFormat(IDeviceReport report)
        {
            StringBuilder sb = new StringBuilder();

            if (report is IAbsolutePositionReport absolutePositionReport)
                sb.AppendLines(GetStringFormat(absolutePositionReport));
            if (report is ITabletReport tabletReport)
                sb.AppendLines(GetStringFormat(tabletReport));
            if (report is IAuxReport auxReport)
                sb.AppendLines(GetStringFormat(auxReport));
            if (report is IEraserReport eraserReport)
                sb.AppendLines(GetStringFormat(eraserReport));
            if (report is IConfidenceReport confidenceReport)
                sb.AppendLines(GetStringFormat(confidenceReport));
            if (report is IHoverReport proximityReport)
                sb.AppendLines(GetStringFormat(proximityReport));
            if (report is ITiltReport tiltReport)
                sb.AppendLines(GetStringFormat(tiltReport));
            if (report is ITouchReport touchReport)
                sb.AppendLines(GetStringFormat(touchReport));
            if (report is IMouseReport mouseReport)
                sb.AppendLines(GetStringFormat(mouseReport));
            if (report is IToolReport toolReport)
                sb.AppendLines(GetStringFormat(toolReport));
            if(report is IAbsoluteWheelReport absoluteWheelReport)
                sb.AppendLines(GetStringFormat(absoluteWheelReport));
            if (report is OutOfRangeReport)
                sb.AppendLine("Out of range");

            return sb.ToString();
        }

        private static IEnumerable<string> GetStringFormat(IAbsolutePositionReport absolutePositionReport)
        {
            yield return $"Position:{absolutePositionReport.Position}";
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

        private static IEnumerable<string> GetStringFormat(IHoverReport proximityReport)
        {
            yield return $"HoverDistance:{proximityReport.HoverDistance}";
        }

        private static IEnumerable<string> GetStringFormat(IConfidenceReport confidenceReport)
        {
            yield return $"Confidence:{confidenceReport.HighConfidence}";
        }

        private static IEnumerable<string> GetStringFormat(ITiltReport tiltReport)
        {
            yield return $"Tilt:{tiltReport.Tilt}";
        }

        private static IEnumerable<string> GetStringFormat(ITouchReport touchReport)
        {
            yield return $"Touch data:";
            foreach (var touch in touchReport.Touches)
                if (touch != null)
                    yield return touch.Value.ToString();
        }

        private static IEnumerable<string> GetStringFormat(IMouseReport mouseReport)
        {
            yield return $"MouseButtons:[{string.Join(" ", mouseReport.MouseButtons)}]";
            yield return $"Scroll:{mouseReport.Scroll}";
        }

        private static IEnumerable<string> GetStringFormat(IToolReport toolReport)
        {
            yield return $"Tool:{Enum.GetName(toolReport.Tool)}";
            yield return $"RawToolID:{toolReport.RawToolID}";
            yield return $"Serial:{toolReport.Serial}";
        }

        private static IEnumerable<string> GetStringFormat(IAbsoluteWheelReport toolReport)
        {
            yield return $"Wheel:{toolReport.WheelPosition?.ToString() ?? "Idle"}";
        }

        private static void AppendLines(this StringBuilder sb, IEnumerable<string> lines)
        {
            foreach (var line in lines)
                sb.AppendLine(line);
        }
    }
}
