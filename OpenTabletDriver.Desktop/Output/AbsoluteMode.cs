using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Absolute Mode")]
    public class AbsoluteMode : AbsoluteOutputMode, IPointerProvider<IAbsolutePointer>
    {
        public override IAbsolutePointer Pointer => SystemInterop.AbsolutePointer;
    }
}
