using System;
using System.Diagnostics;
using System.Linq;

namespace TabletDriverCLI
{
    public static class Extensions
    {
        public static string Divider => string.Concat(Enumerable.Repeat('-', Console.BufferWidth));

        public static string CenterInDivider(this string content, bool addSpace = true)
        {
            if (Console.BufferWidth == 0)
                return content;

            var text = content;
            if (addSpace)
                text = ' ' + content + ' ';
            if ((content.Length % 2) == 0)
            {
                var halfDivider = string.Concat(Enumerable.Repeat('-', (Console.BufferWidth - text.Length) / 2));
                return halfDivider + text + halfDivider;
            }
            else
            {
                var halfDivider = string.Concat(Enumerable.Repeat('-', (Console.BufferWidth - text.Length - 1) / 2));
                return halfDivider + text + halfDivider + '-';
            }
        }

        public static string LeftAlignInDivider(this string content, int columns = 3, bool addSpace = true)
        {
            if (Console.BufferWidth == 0)
                return content;

            var text = content;
            if (addSpace)
                text = ' ' + content + ' ';
            var columnGap = string.Concat(Enumerable.Repeat('-', columns));
            var remainderGap = string.Concat(Enumerable.Repeat('-', Console.BufferWidth - (columns + text.Length)));
            return columnGap + text + remainderGap;
        }

        public static void Log(string prefix, string text)
        {
            Trace.WriteLine($"[{prefix}] {text}");
        }

        public static string Remainder(this string[] tokens, int index)
        {
            return string.Join(" ", tokens.Skip(index));
        }
    }
}