using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace OpenTabletDriver.Plugin.Tablet
{
    public static class ByteExtensions
    {
        /// <summary>
        /// Helper function to check if a bit is set in a byte
        /// </summary>
        /// <param name="a">The byte to check the bit against</param>
        /// <param name="bit">Bit index, 0-indexed (<c>0</c> to <c>7</c>)</param>
        /// <returns><c>true</c> if <paramref name="bit"/> is set</returns>
        /// <remarks>For performance, the bit index is not checked for a valid range</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public static bool IsBitSet(this byte a, int bit)
        {
            return (a & (1 << bit)) != 0;
        }
    }
}
