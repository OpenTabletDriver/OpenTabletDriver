using System;
using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Artist Mode"), SupportedPlatform(PluginPlatform.Linux)]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class LinuxArtistMode(IPressureHandler pressureHandler) : AbsoluteOutputMode
    {
        public IPressureHandler? VirtualTablet { get; set; } = pressureHandler;

        public override IAbsolutePointer? Pointer
        {
            set => throw new NotSupportedException();
            get => (IAbsolutePointer)(VirtualTablet ?? throw new InvalidOperationException($"{nameof(VirtualTablet)} was not properly injected by DI"));
        }
    }
}
