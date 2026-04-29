using System.Diagnostics.CodeAnalysis;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Absolute Mode")]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public class AbsoluteMode : AbsoluteOutputMode
    {
        [Resolved]
        public override IAbsolutePointer? Pointer { set; get; }
    }
}
