using OpenTabletDriver.Attributes;
using OpenTabletDriver.Daemon.Library.Interop.Input.Absolute;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Library.Output
{
    [PluginName("Artist Mode"), SupportedPlatform(SystemPlatform.Linux)]
    public class LinuxArtistMode : AbsoluteOutputMode, IMouseButtonSource
    {
        public LinuxArtistMode(
            InputDevice tablet,
            EvdevVirtualTablet pressureHandler,
            ISettingsProvider settingsProvider
        ) : base(tablet, pressureHandler)
        {
            settingsProvider.Inject(this);
        }

        public IMouseButtonHandler MouseButtonHandler => Pointer;
    }
}
