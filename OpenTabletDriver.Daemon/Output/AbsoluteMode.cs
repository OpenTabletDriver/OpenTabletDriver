using OpenTabletDriver.Attributes;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Output
{
    [PluginName("Absolute Mode")]
    public class AbsoluteMode : AbsoluteOutputMode, IMouseButtonSource
    {
        public AbsoluteMode(
            InputDevice tablet,
            IAbsolutePointer absolutePointer,
            ISettingsProvider settingsProvider
        ) : base(tablet, absolutePointer)
        {
            settingsProvider.Inject(this);
        }


        public IMouseButtonHandler MouseButtonHandler => Pointer;
    }
}
