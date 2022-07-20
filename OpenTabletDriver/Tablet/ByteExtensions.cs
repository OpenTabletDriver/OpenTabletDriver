using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// Utility extensions for bytes.
    /// </summary>
    [PublicAPI]
    public static class ByteExtensions
    {
        /// <summary>
        /// Returns if a bit is set in a byte.
        /// </summary>
        /// <param name="a">The byte to index.</param>
        /// <param name="bit">The index of the bit.</param>
        /// <returns>
        /// Boolean representing if the bit was set.
        /// </returns>
        public static bool IsBitSet(this byte a, int bit)
        {
            return (a & (1 << bit)) != 0;
        }
    }
}
