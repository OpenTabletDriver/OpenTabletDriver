using System;
using OpenTabletDriver.Interop.Input.Tablet;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Output
{
    [PluginName("Artist Mode"), SupportedPlatform(PluginPlatform.Linux)]
    public class LinuxArtistMode : AbsoluteOutputMode
    {
        private Lazy<EvdevPenHandler> penHandler = new Lazy<EvdevPenHandler>();
        public override IVirtualTablet VirtualTablet => penHandler.Value;
    }
}