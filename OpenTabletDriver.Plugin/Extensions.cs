using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace OpenTabletDriver.Plugin
{
    [PublicAPI]
    public static class Extensions
    {
        extension(string str)
        {
            [Pure]
            public string Elide(int maxLength, string elisionMarker = "...")
            {
                ArgumentOutOfRangeException.ThrowIfNegative(maxLength - elisionMarker.Length);
                return str.Length > maxLength
                    ? str[..(maxLength - elisionMarker.Length)] + elisionMarker
                    : str;
            }
        }

        extension(ReadOnlySpan<byte> data)
        {
            /// <summary>
            /// Convert byte span to a hex-formatted string
            /// </summary>
            /// <param name="upperCase">if <c>true</c>, output string is capitalized</param>
            /// <param name="wrap">If <c>true</c>, wrap string in curly braces</param>
            /// <param name="spaced">If <c>true</c>, each byte should be separated by spaces</param>
            /// <returns>A formatted string, e.g. <c>"{ 00 02 F0 }"</c> or <c>"0002f0"</c></returns>
            [Pure]
            public string PrintHex(bool upperCase = false, bool wrap = false, bool spaced = false)
            {
                // set up hex formatting string
                var formatSb = new StringBuilder("{0:", 8);

                formatSb.Append(upperCase ? 'X' : 'x');
                formatSb.Append("2}");

                if (spaced)
                    formatSb.Append(' '); // e.g. "{0:X2} "
                // else e.g. "{0:X2}"

                string format = formatSb.ToString();

                // initialize with exact size and appropriate initial value
                int segmentSizeMultiplier = 2 + (spaced ? 1 : 0);
                string startString = string.Empty;
                if (wrap)
                    startString = spaced ? "{ " : "{";

                var exactLength = data.Length * segmentSizeMultiplier + (wrap ? 3 : 0);

                var sb = new StringBuilder(startString, exactLength);

                foreach (byte b in data)
                    sb.AppendFormat(format, b);

                if (wrap)
                    sb.Append('}');
                else if (spaced)
                    sb.Remove(sb.Length - 1, 1);

                return sb.ToString();
            }

            /// <remarks>
            /// If you're working with a file, consider
            /// <see cref="M:OpenTabletDriver.Plugin.Extensions.extension(System.IO.Stream).GetSHA256"/>
            /// instead to avoid large allocations
            /// </remarks>
            [Pure]
            public ReadOnlySpan<byte> GetSHA256() => SHA256.HashData(data);
        }

        extension(Stream stream)
        {
            [Pure]
            public ReadOnlySpan<byte> GetSHA256()
            {
                long oldPos = stream.Position;

                stream.Position = 0; // must reset otherwise hashing becomes incomplete

                ReadOnlySpan<byte> hashData = SHA256.HashData(stream);

                stream.Position = oldPos;

                return hashData;
            }

            [Pure]
            public bool VerifySHA256(ReadOnlySpan<byte> hash, out ReadOnlySpan<byte> actual)
            {
                Debug.Assert(hash.Length == 32);
                actual = stream.GetSHA256();
                return actual.SequenceEqual(hash);
            }

            [Pure]
            public bool VerifySHA256(string hash, out ReadOnlySpan<byte> actual) =>
                stream.VerifySHA256(Convert.FromHexString(hash.ToLower().Replace(" ", "")), out actual);
        }
    }
}
