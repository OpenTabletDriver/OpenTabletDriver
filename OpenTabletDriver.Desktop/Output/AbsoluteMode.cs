using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Absolute Mode")]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public class AbsoluteMode(IAbsolutePointer absolutePointer) : AbsoluteOutputMode
    {
        public override IAbsolutePointer? Pointer { set; get; } = absolutePointer;
    }
}
