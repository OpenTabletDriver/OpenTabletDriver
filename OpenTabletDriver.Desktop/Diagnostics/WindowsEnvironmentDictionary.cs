namespace OpenTabletDriver.Desktop.Diagnostics
{
    public sealed class WindowsEnvironmentDictionary : EnvironmentDictionary
    {
        private static readonly string[] EnvironmentVariables = new[]
        {
            "USER",
            "TEMP",
            "TMP",
            "TMPDIR"
        };

        public WindowsEnvironmentDictionary() : base(EnvironmentVariables)
        {
        }
    }
}
