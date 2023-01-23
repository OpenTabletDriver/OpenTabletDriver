namespace OpenTabletDriver.Daemon.Diagnostics
{
    public sealed class WindowsEnvironmentDictionary : EnvironmentDictionary
    {
        private static readonly string[] EnvironmentVariables =
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
