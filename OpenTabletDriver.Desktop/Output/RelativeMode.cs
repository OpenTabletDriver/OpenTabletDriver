using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Relative Mode")]
    public class RelativeMode : RelativeOutputMode, IPointerProvider<IRelativePointer>
    {
        [Resolved]
        public override IRelativePointer Pointer { set; get; }
    }
}
