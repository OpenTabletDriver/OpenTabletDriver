using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Relative Mode")]
    public class RelativeMode : RelativeOutputMode, IPointerProvider<IRelativePointer>
    {
        public override IRelativePointer Pointer => SystemInterop.RelativePointer;
    }
}
