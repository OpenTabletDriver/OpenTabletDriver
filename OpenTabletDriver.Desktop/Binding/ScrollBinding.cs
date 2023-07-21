using System.Numerics;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Platform.Pointer;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class ScrollBinding : IStateBinding
    {
        private const string PLUGIN_NAME = "Scroll Binding";

        private readonly IMouseButtonHandler _mouseButtonHandler;

        public ScrollBinding(IMouseButtonHandler mouseButtonHandler, ISettingsProvider settingsProvider)
        {
            _mouseButtonHandler = mouseButtonHandler;

            settingsProvider.Inject(this);
        }

        [Setting(nameof(Vertical))]
        public int Vertical { set; get; }

        [Setting(nameof(Horizontal), "The amount of ticks horizontally to scroll.")]
        public int Horizontal { set; get; }

        public void Press(IDeviceReport report)
        {
            _mouseButtonHandler.Scroll(new Vector2(Horizontal, Vertical));
        }

        public void Release(IDeviceReport report)
        {
            // This is a
        }
    }
}
