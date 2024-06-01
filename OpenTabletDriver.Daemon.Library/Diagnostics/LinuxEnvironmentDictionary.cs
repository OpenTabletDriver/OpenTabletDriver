namespace OpenTabletDriver.Daemon.Diagnostics
{
    public sealed class LinuxEnvironmentDictionary : EnvironmentDictionary
    {
        private static readonly string[] EnvironmentVariables = new[]
        {
            "DISPLAY",
            "WAYLAND_DISPLAY",
            "PWD",
            "PATH"
        };

        public LinuxEnvironmentDictionary() : base(EnvironmentVariables)
        {
        }
    }
}
