using System;
using System.Globalization;

namespace OpenTabletDriver.UX.Tools
{
    public static class ParseTools
    {
        public static float? ToNullableFloat(string str) => float.TryParse(str, out var val) ? val : null;
        public static float ToFloat(string str) => ToNullableFloat(str) ?? 0f;

        public static int? ToNullableInt(string str) => int.TryParse(str, out var val) ? val : null;
        public static int ToInt(string str) => ToNullableInt(str) ?? 0;

        public static uint? ToNullableUInt(string str) => uint.TryParse(str, out var val) ? val : null;
        public static uint ToUInt(string str) => ToNullableUInt(str) ?? 0;

        public static bool TryGetHexValue(string str, out byte value) => byte.TryParse(str.Replace("0x", string.Empty), NumberStyles.HexNumber, null, out value);

        public static string ToHexString(byte[] value) => "0x" + BitConverter.ToString(value).Replace("-", " 0x");

        public static byte[]? ToByteArray(string hex)
        {
            var raw = hex.Split(' ');
            byte[] buffer = new byte[raw.Length];
            for (int i = 0; i < raw.Length; i++)
            {
                if (TryGetHexValue(raw[i], out var val))
                    buffer[i] = val;
                else
                    return null;
            }
            return buffer;
        }
    }
}
