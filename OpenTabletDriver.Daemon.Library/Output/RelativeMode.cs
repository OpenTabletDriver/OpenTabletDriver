using OpenTabletDriver.Attributes;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Output
{
    [PluginName("Relative Mode")]
    public class RelativeMode : RelativeOutputMode, IMouseButtonSource
    {
        public RelativeMode(
            InputDevice tablet,
            IRelativePointer relativePointer,
            ISettingsProvider settingsProvider
        ) : base(tablet, relativePointer)
        {
            settingsProvider.Inject(this);
        }

        public IMouseButtonHandler MouseButtonHandler => Pointer;
    }
}
