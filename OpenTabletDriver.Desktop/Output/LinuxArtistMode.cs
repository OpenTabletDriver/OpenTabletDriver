using System;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Artist Mode"), SupportedPlatform(PluginPlatform.Linux)]
    public class LinuxArtistMode : AbsoluteOutputMode, IPointerProvider<IAbsolutePointer>
    {
        [Resolved]
        public IPressureHandler VirtualTablet { get; set; }

        public override IAbsolutePointer Pointer
        {
            set => throw new NotSupportedException();
            get => (IAbsolutePointer)VirtualTablet;
        }
    }
}
