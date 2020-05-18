using System;
using System.Runtime.InteropServices;

namespace TabletDriverPlugin.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class SupportedPlatformAttribute : Attribute
    {
        public SupportedPlatformAttribute(PluginPlatform platform)
        {
            this.Platform = platform;
        }

        public PluginPlatform Platform { private set; get; }

        public bool IsCurrentPlatform => 
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Platform.HasFlag(PluginPlatform.Windows) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && Platform.HasFlag(PluginPlatform.Linux) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && Platform.HasFlag(PluginPlatform.MacOS);
    }
}