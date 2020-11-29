using System.Numerics;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Absolute Mode")]
    public class AbsoluteMode : AbsoluteOutputMode, IPointerOutputMode<IAbsolutePointer>
    {
        public override IVirtualScreen VirtualScreen => Platform.VirtualScreen;
        public override IAbsolutePointer Pointer => Platform.VirtualTablet;
    }
}
