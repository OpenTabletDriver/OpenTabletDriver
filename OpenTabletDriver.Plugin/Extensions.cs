namespace OpenTabletDriver.Plugin
{
    public static class Extensions
    {
        extension(string str)
        {
            public string Elide(int maxLength, string elisionMarker = "...") =>
                str.Length > maxLength
                    ? str[..(maxLength - elisionMarker.Length)] + elisionMarker
                    : str;
        }
    }
}
