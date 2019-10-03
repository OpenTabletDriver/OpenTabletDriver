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
            Trace.WriteLine($"[{prefix.ToUpper()}] {text}");
        }

        public static void Log(Exception ex)
        {
            Log(ex.GetType().Name, ex.Message.Replace(Environment.NewLine, "; "));
        }

        public static void DebugLog(string prefix, string text)
        {
            Debug.WriteLine($"[{prefix.ToUpper()}] {text}");
        }

        public static void DebugLog(Exception ex)
        {
            DebugLog(ex.GetType().Name, ex.Message);
        }

        public static string Remainder(this string[] tokens, int index)
        {
            return string.Join(" ", tokens.Skip(index));
        }

        public static T TryGetResult<T>(Func<T> method)
        {
            try
            {
                var result = method.Invoke();
                DebugLog("TRYGET", "Got result: " + result);
                return result;
            }
            catch (Exception ex)
            {
                DebugLog(ex);
                return default(T);
            }
        }
    }
}