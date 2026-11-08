using System;

namespace OpenTabletDriver.Plugin.Attributes
{
    /// <summary>
    /// Locks a class, struct, or interface to a specific platform.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class SupportedPlatformAttribute : Attribute
    {
        private readonly string _platform;
        public SupportedPlatformAttribute(string platform)
        {
            _platform = platform;
        }

        public bool IsCurrentPlatform => _platform.ToLowerInvariant() switch
        {
            "windows" => OperatingSystem.IsWindows(),
            "linux" => OperatingSystem.IsLinux(),
            "macos" => OperatingSystem.IsMacOS(),
            "freebsd" => OperatingSystem.IsFreeBSD(),
            _ => false
        };
    }
}
