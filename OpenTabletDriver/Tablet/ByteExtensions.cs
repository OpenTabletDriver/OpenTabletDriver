namespace OpenTabletDriver.Tablet
{
    public static class ByteExtensions
    {
        public static bool IsBitSet(this byte a, int bit)
        {
            return (a & (1 << bit)) != 0;
        }
    }
}