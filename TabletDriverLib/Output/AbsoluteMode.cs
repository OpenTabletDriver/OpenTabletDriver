using TabletDriverLib.Interop;
using TabletDriverPlugin.Attributes;
using TabletDriverPlugin.Output;
using TabletDriverPlugin.Platform.Pointer;

namespace TabletDriverLib.Output
{
    [PluginName("Absolute Mode")]
    public class AbsoluteMode : AbsoluteOutputMode
    {
        public override IVirtualTablet VirtualTablet => Platform.VirtualTablet;
    }
}