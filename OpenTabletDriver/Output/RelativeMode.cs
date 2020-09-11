using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Output
{
    [PluginName("Relative Mode")]
    public class RelativeMode : RelativeOutputMode
    {
        public override IVirtualMouse VirtualMouse => Platform.VirtualMouse;
    }
}