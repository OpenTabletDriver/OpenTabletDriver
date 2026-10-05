using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Relative Mode")]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class RelativeMode(IRelativePointer relativePointer) : RelativeOutputMode
    {
        public override IRelativePointer? Pointer { set; get; } = relativePointer;
    }
}
