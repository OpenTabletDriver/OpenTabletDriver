using OpenTabletDriver.Attributes;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Output
{
    [PluginName("Artist Mode"), SupportedPlatform(SystemPlatform.Linux)]
    public class LinuxArtistMode : AbsoluteOutputMode, IMouseButtonSource
    {
        public LinuxArtistMode(
            InputDevice tablet,
            IPressureHandler pressureHandler,
            ISettingsProvider settingsProvider
        ) : base(tablet, pressureHandler)
        {
            settingsProvider.Inject(this);
        }

        public IMouseButtonHandler MouseButtonHandler => Pointer;
    }
}
