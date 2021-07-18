using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class MouseScrollBinding : IStateBinding
    {
        private const string PLUGIN_NAME = "Mouse Scroll Binding";

        private const string HORIZONTAL_TOOLTIP = "The amount of ticks to scroll on activation.\n" +
            "A positive value indicates scrolling right.\n" +
            "A negative value indicates scrolling left.";

        private const string VERTICAL_TOOLTIP = "The amount of ticks to scroll on activation in the horizontal direction.\n" +
            "A positive value indicates scrolling up.\n" +
            "A negative value indicates scrolling down.";

        [Resolved]
        public IScrollablePointer Pointer { set; get; }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            Pointer?.Scroll(new Vector2(Horizontal, Vertical));
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
        }

        [Property("Horizontal"), ToolTip(HORIZONTAL_TOOLTIP), DefaultPropertyValue(0)]
        public float Horizontal { set; get; }

        [Property("Vertical"), ToolTip(VERTICAL_TOOLTIP), DefaultPropertyValue(0)]
        public float Vertical { set; get; }

        public override string ToString() => $"{PLUGIN_NAME}: ({Horizontal},{Vertical})";
    }
}