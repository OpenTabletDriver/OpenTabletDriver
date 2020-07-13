using System;
using System.Collections.Generic;
using System.Linq;
using TabletDriverLib.Interop;
using TabletDriverPlugin;
using TabletDriverPlugin.Attributes;
using TabletDriverPlugin.Output;
using TabletDriverPlugin.Platform.Pointer;
using TabletDriverPlugin.Tablet;

namespace TabletDriverLib.Output
{
    [PluginName("Relative Mode")]
    public class RelativeMode : RelativeOutputMode
    {
        public override IPointerHandler PointerHandler => Platform.MouseHandler;
    }
}