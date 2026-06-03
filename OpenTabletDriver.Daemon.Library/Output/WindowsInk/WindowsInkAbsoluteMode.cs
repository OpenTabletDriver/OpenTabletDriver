using System.ComponentModel;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Library.Output.WindowsInk
{
    [PluginName("Windows Ink Absolute Mode"), SupportedPlatform(SystemPlatform.Windows)]
    public class WindowsInkAbsoluteMode : AbsoluteOutputMode, IMouseButtonSource
    {
        private readonly WindowsInkPointer _windowsInkPointer;
        private bool _sync = true;
        private bool _forcedSync;

        public WindowsInkAbsoluteMode(
            InputDevice tablet,
            IVirtualScreen virtualScreen,
            ISettingsProvider settingsProvider
        ) : base(tablet, new WindowsInkPointer(virtualScreen))
        {
            _windowsInkPointer = (WindowsInkPointer)Pointer;
            settingsProvider.Inject(this);
            _windowsInkPointer.Sync = _sync;
            _windowsInkPointer.ForcedSync = _forcedSync;
        }

        [Setting("Sync OS Cursor", "Synchronize the normal OS cursor with the Windows Ink pen position when the pen leaves range."), DefaultValue(true)]
        public bool Sync
        {
            set
            {
                _sync = value;
                if (_windowsInkPointer is not null)
                    _windowsInkPointer.Sync = value;
            }
            get => _sync;
        }

        [Setting("Forced Sync", "Synchronize the normal OS cursor on every Windows Ink report."), DefaultValue(false)]
        public bool ForcedSync
        {
            set
            {
                _forcedSync = value;
                if (_windowsInkPointer is not null)
                    _windowsInkPointer.ForcedSync = value;
            }
            get => _forcedSync;
        }

        public IMouseButtonHandler MouseButtonHandler => _windowsInkPointer;
    }
}
