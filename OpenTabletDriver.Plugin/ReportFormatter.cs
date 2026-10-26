using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin
{
    public static class ReportFormatter
    {
        /// <summary>
        /// Dictionary for defining StringBuilder actions based on <see cref="DeviceReport"/>
        /// </summary>
        private static readonly Dictionary<IsValidForParsing, ParseMe> s_Formats = new()
        {
           { // Position
               (in x) => x.PenPosition.HasValue,
               (in x, sb, ender) => ender(sb.Append($"Position:{x.PenPosition.GetValueOrDefault()}"))
           },
           { // Tablet (pressure)
               (in x) => x.PenPressure.HasValue,
               (in x, sb, ender) => ender(sb.Append($"Pressure:{x.PenPressure.GetValueOrDefault()}"))
           },
           { // Tablet (pen buttons)
               (in x) => x.PenButtons != null,
               (in x, sb, ender) => ender(sb.appendJoinWrapped("PenButtons", [.. x.PenButtons!.Span]))
           },
           { // Aux
               (in x) => x.AuxButtons != null,
               (in x, sb, ender) => ender(sb.appendJoinWrapped("AuxButtons", [.. x.AuxButtons!.Span]))
           },
           { // Eraser
                (in x) => x.Eraser.HasValue,
                (in x, sb, ender) => ender(sb.Append($"Eraser:{x.Eraser.GetValueOrDefault()}"))
           },
           { // Proximity: NearProximity
                (in x) => x.NearProximity.HasValue,
                (in x, sb, ender) => ender(sb.Append($"NearProximity:{x.NearProximity.GetValueOrDefault()}"))
           },
           { // Proximity: HoverDistance
               (in x) => x.HoverDistance.HasValue,
               (in x, sb, ender) => ender(sb.Append($"HoverDistance:{x.HoverDistance.GetValueOrDefault()}"))
           },
           { // Tilt
               (in x) => x.Tilt.HasValue,
               (in x, sb, ender) => ender(sb.Append($"Tilt:{x.Tilt.GetValueOrDefault()}"))
           },
           { // Rotation
               (in x) => x.Rotation.HasValue,
               (in x, sb, ender) => ender(sb.Append($"Rotation:{x.Rotation.GetValueOrDefault()}"))
           },
           { // Touch
               (in x) => x.Touches != null,
               (in x, sb, ender) => ender(sb.appendJoinWrappedHideNull("Touch data", [..x.Touches!.Span]))
           },
           { // Absolute Wheel report
               (in x) => x.WheelPositions != null,
               (in x, sb, ender) =>
                   sb.appendJoinWithIndexWrapped("Abs. Wheel", [..x.WheelPositions!.Span], ender, "Idle")
           },
           { // Relative Wheel report
               (in x) => x.WheelDeltas != null,
               (in x, sb, ender) =>
                   sb.appendJoinWithIndexWrapped("Rel. Wheel", [..x.WheelPositions!.Span], ender)
           },
           { // Wheel buttons
               (in x) => x.WheelButtons != null,
               (in x, sb, ender) =>
                   sb.appendJoinWithIndexWrapped("Wheel", x.WheelButtons!, ender, headerTrailer: "Buttons")
           },
           { // Mouse Buttons
               (in x) => x.MouseButtons != null,
               (in x, sb, ender) => ender(sb.appendJoinWrapped("Mouse Buttons", [..x.MouseButtons!.Span]))
           },
           { // Mouse scroll
               (in x) => x.MouseScroll.HasValue,
               (in x, sb, ender) => ender(sb.Append($"Mouse Scroll:{x.MouseScroll.GetValueOrDefault()}"))
           },
           { // Tool Type
               (in x) => x.ToolType.HasValue,
               (in x, sb, ender) => ender(sb.Append($"Tool Type:{Enum.GetName(x.ToolType.GetValueOrDefault())}"))
           },
           { // Tool ID
               (in x) => x.RawToolID.HasValue,
               (in x, sb, ender) => ender(sb.Append($"Tool ID:{x.RawToolID.GetValueOrDefault()}"))
           },
           { // Tool Serial
               (in x) => x.ToolSerial.HasValue,
               (in x, sb, ender) => ender(sb.Append($"Tool Serial:{x.ToolSerial.GetValueOrDefault()}"))
           },
           { // Out of range
               (in x) => x.OutOfRange,
               (in _, sb, ender) => ender(sb.Append("Pen is out of range"))
           },
        };

        public static string GetStringRawAsHex(ReadOnlySpan<byte> bytes) => Convert.ToHexString(bytes);

        public static string GetStringRawAsBinary(ReadOnlySpan<byte> bytes)
        {
            var sb = new StringBuilder(bytes.Length * (8 + 1)); // 8 == 1 byte in binary, 1 for the whitespace

            foreach (byte b in bytes)
                sb.Append($"{b:B8} ");

            return sb.ToString();
        }

        public static string GetStringFormat(scoped in DeviceReport report)
            => formatDeviceReport(report, x => x.AppendLine()).ToString();

        public static string GetStringFormatOneLine(TabletConfiguration tabletProperties, scoped in DeviceReport report, TimeSpan delta, string reportType)
        {
            var sb = new StringBuilder(1024); // some initial size is better than no initial size

            sb.Append($"Device:{tabletProperties.Name}");
            sb.Append($", {{ {GetStringRawAsHex(report.Raw)} }}");
            sb.Append($", Delta:{delta.TotalMilliseconds:F3}ms");

            formatDeviceReport(report, x => x.Append(", "), sb);

            reportType = reportType.StartsWith("OpenTabletDriver.Configurations.Parsers")
                ? reportType.Split('.').Last()
                : reportType;
            sb.Append(", ReportType:" + reportType);

            return sb.ToString();
        }

        /// <param name="sb"><see cref="StringBuilder"/> responsible for curating the report format string</param>
        extension(StringBuilder sb)
        {
            /// <summary>
            /// See <see cref="ReportFormatter.appendJoinWrapped{T}"/> - this one filters out nulls from <paramref name="values"/>
            /// </summary>
            private StringBuilder appendJoinWrappedHideNull<T>(ReadOnlySpan<char> header, IEnumerable<T?> values) =>
                sb.appendJoinWrapped(header, values.Where(x => x != null));

            private StringBuilder appendJoinWrappedReplaceNull<T>(ReadOnlySpan<char> header, IEnumerable<T?> values, string nullReplacement) =>
                sb.appendJoinWrapped(header, values.Select(x => x?.ToString() ?? nullReplacement));

            /// <summary>
            /// Custom join implementation of <see cref="ReportFormatter.appendJoinWrapped{T}"/>. Adds index number to the header.
            /// </summary>
            /// <param name="header">String to be appended first to the string</param>
            /// <param name="values">The values to be joined in</param>
            /// <param name="lineEnder"><see cref="StringBuilder"/>-wrapping function to call at the end of each line</param>
            /// <param name="nullReplacement">String value to replace null values, if any</param>
            private StringBuilder appendJoinWithIndexWrapped<T>(ReadOnlySpan<char> header,
                IEnumerable<T?> values,
                Func<StringBuilder, StringBuilder> lineEnder,
                string nullReplacement = "")
            {
                int i = 1;

                foreach (var v in values)
                    lineEnder(sb.appendWrapped($"{header} {i++}", v?.ToString() ?? nullReplacement));

                return sb;
            }

            /// <summary>
            /// <see cref="M:OpenTabletDriver.Plugin.ReportFormatter.extension(System.Text.StringBuilder).appendJoinWithIndexWrapped``1(System.ReadOnlySpan{System.Char},System.Collections.Generic.IEnumerable{``0},System.Func{System.Text.StringBuilder,System.Text.StringBuilder},System.String)"/>
            /// but with 2D <see cref="MemoryOwner{T}"/> handling
            /// </summary>
            /// <param name="header">String to be appended first to the string</param>
            /// <param name="memoryOwner">The values to be joined in</param>
            /// <param name="lineEnder"><see cref="StringBuilder"/>-wrapping function to call at the end of each line</param>
            /// <param name="nullReplacement">String value to replace null values, if any</param>
            /// <param name="headerTrailer">String to be appended after the <paramref name="header"/> and the index</param>
            /// <typeparam name="T">The template type of the inner memory owner</typeparam>
            private StringBuilder appendJoinWithIndexWrapped<T>(ReadOnlySpan<char> header,
                MemoryOwner<MemoryOwner<T?>> memoryOwner,
                Func<StringBuilder, StringBuilder> lineEnder,
                string nullReplacement = "",
                ReadOnlySpan<char> headerTrailer = "")
            {
                int i = 1;

                foreach (var v in memoryOwner.Span)
                {
                    string curHeader = headerTrailer.IsEmpty
                        ? $"{header} {i++}"
                        : $"{header} {i++} {headerTrailer}";

                    lineEnder(sb.appendJoinWrappedReplaceNull(curHeader, [.. v.Span], nullReplacement));
                }

                return sb;
            }

            /// <summary>
            /// Updates the underlying <see cref="StringBuilder"/> with data based on a header and its values provided
            /// </summary>
            /// <param name="header">The heading title of the line</param>
            /// <param name="values">The <typeparam name="T"> enumerable</typeparam></param>
            /// <typeparam name="T">The underlying type of the objects in the enumerable</typeparam>
            /// <returns><c>"{header}:[{values}]"</c> appended to the <see cref="StringBuilder"/></returns>
            private StringBuilder appendJoinWrapped<T>(ReadOnlySpan<char> header, IEnumerable<T> values) =>
                sb.Append(header)
                    .Append(":[")
                    .AppendJoin(' ', values)
                    .Append(']');

            private StringBuilder appendWrapped(ReadOnlySpan<char> header, ReadOnlySpan<char> value) =>
                sb.Append(header)
                    .Append(':')
                    .Append(value);
        }

        private static StringBuilder formatDeviceReport(scoped in DeviceReport report, Func<StringBuilder, StringBuilder> sbFunc, StringBuilder? sb = null)
        {
            sb ??= new StringBuilder(1024);
            foreach (var format in s_Formats)
                if (format.Key(in report))
                    format.Value(in report, sb, sbFunc);

            return sb;
        }

        /// <summary>
        /// Checks device report for whether associated <see cref="ParseMe"/> handler should be run.
        /// </summary>
        private delegate bool IsValidForParsing(scoped in DeviceReport report);

        /// <summary>
        /// String format handler for associated report types.
        /// </summary>
        private delegate StringBuilder ParseMe(scoped in DeviceReport report, StringBuilder sb, Func<StringBuilder, StringBuilder> lineEnder);
    }
}
