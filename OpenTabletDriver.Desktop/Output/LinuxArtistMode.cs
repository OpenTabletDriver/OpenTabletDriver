using System;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Artist Mode"), SupportedPlatform(PluginPlatform.Linux)]
    public class LinuxArtistMode : AbsoluteOutputMode, IPointerOutputMode<IAbsolutePointer>
    {
        private Lazy<EvdevVirtualTablet> penHandler = new Lazy<EvdevVirtualTablet>();

        public override IVirtualScreen VirtualScreen => throw new NotImplementedException();
        public override IAbsolutePointer Pointer => penHandler.Value;
    }
}
