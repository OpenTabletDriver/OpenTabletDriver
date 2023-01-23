using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Daemon.Interop.Display;
using OpenTabletDriver.Daemon.Interop.Environment;
using OpenTabletDriver.Daemon.Interop.Input.Absolute;
using OpenTabletDriver.Daemon.Interop.Input.Keyboard;
using OpenTabletDriver.Daemon.Interop.Input.Relative;
using OpenTabletDriver.Daemon.Interop.Timer;
using OpenTabletDriver.Daemon.Updater;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Environment;
using OpenTabletDriver.Platform.Keyboard;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Interop
{
    using static ServiceDescriptor;

    public sealed class DesktopMacOSServiceCollection : DesktopServiceCollection
    {
        public DesktopMacOSServiceCollection() : base()
        {
            this.AddServices(new[] {
                Transient<ITimer, MacOSTimer>(),
                Transient<IAbsolutePointer, MacOSAbsolutePointer>(),
                Transient<IRelativePointer, MacOSRelativePointer>(),
                Transient<IVirtualKeyboard, MacOSVirtualKeyboard>(),
                Transient<IKeysProvider, MacOSKeysProvider>(),
                Transient<IVirtualScreen, MacOSDisplay>(),
                Transient<IEnvironmentHandler, MacOSEnvironmentHandler>(),
                Transient<IUpdater, MacOSUpdater>()
            });
        }
    }
}
