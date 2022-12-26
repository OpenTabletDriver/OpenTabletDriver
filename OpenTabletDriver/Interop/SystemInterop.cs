using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace OpenTabletDriver.Interop
{
    /// <summary>
    /// Provides system platform detection.
    /// </summary>
    [PublicAPI]
    public static class SystemInterop
    {
        /// <summary>
        /// The currently running system platform.
        /// </summary>
        public static SystemPlatform CurrentPlatform
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return SystemPlatform.Windows;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return SystemPlatform.Linux;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return SystemPlatform.MacOS;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                    return SystemPlatform.FreeBSD;

                return default;
            }
        }
    }
}
