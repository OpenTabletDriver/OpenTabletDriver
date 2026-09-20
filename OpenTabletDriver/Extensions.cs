using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver
{
    internal static class Extensions
    {
        public static bool TryGet<TSource, TValue>(this TSource source, Func<TSource, TValue> predicate, [NotNullWhen(true)] out TValue? value)
        {
            try
            {
                value = predicate(source);
                Debug.Assert(value != null);
                return true;
            }
            catch (Exception ex)
            {
                Log.Exception(ex, LogLevel.Debug);
            }
            value = default;
            return false;
        }

        public static TValue SafeGet<TSource, TValue>(this TSource source, Func<TSource, TValue> predicate, TValue fallback) =>
            source.TryGet(predicate, out var value) ? value : fallback;

        /// <summary>
        /// Converts a byte span to a hex-value pretty-printed string
        /// </summary>
        /// <param name="data">Bytes to pretty print</param>
        /// <param name="wrap">If <c>true</c>, wrap string in curly braces</param>
        /// <returns>A pretty-printed string, e.g. <c>"{ 00 02 F0 }"</c></returns>
        public static string PrettyPrintHex(ReadOnlySpan<byte> data, bool wrap = true)
        {
            var sb = new StringBuilder(data.Length * 3 + (wrap ? 3 : 0));
            foreach (byte b in data)
                sb.Append($"{b:X2} ");

            if (wrap)
            {
                sb.Insert(0, "{ ");
                sb.Append('}');
            }
            else sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
    }
}
