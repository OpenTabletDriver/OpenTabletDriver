using System;
using System.Text;

namespace OpenTabletDriver.Plugin
{
    public static class Extensions
    {
        /// <summary>
        /// Converts a byte span to a hex-value pretty-printed string
        /// </summary>
        /// <param name="data">Bytes to pretty print</param>
        /// <param name="wrap">If <c>true</c>, wrap string in curly braces</param>
        /// <returns>A pretty-printed string, e.g. <c>"{ 00 02 F0 }"</c></returns>
        public static string PrettyPrintHex(this ReadOnlySpan<byte> data, bool wrap = true)
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
