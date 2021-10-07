using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Absolute Mode")]
    public class AbsoluteMode : AbsoluteOutputMode, IPointerProvider<IAbsolutePointer>
    {
        public AbsoluteMode(IAbsolutePointer pointer)
        {
            Pointer = pointer;
        }

        public override IAbsolutePointer Pointer { set; get; }
    }
}
