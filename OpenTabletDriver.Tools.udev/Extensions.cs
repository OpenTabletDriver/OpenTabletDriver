namespace OpenTabletDriver.Tools.udev
{
    internal static class Extensions
    {
        public static string ToHexFormat(this int value) => value.ToString("x2").PadLeft(4, '0');
    }
}
