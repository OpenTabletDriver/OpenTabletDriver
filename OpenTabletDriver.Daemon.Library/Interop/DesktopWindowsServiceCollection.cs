using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Daemon.Diagnostics;
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

    public sealed class DesktopWindowsServiceCollection : DesktopServiceCollection
    {
        public DesktopWindowsServiceCollection() : base()
        {
            this.AddServices(new[] {
                Transient<IEnvironmentHandler, WindowsEnvironmentHandler>(),
                Transient<EnvironmentDictionary, WindowsEnvironmentDictionary>(),
                Transient<ITimer, WindowsTimer>(),
                Transient<IAbsolutePointer, WindowsAbsolutePointer>(),
                Transient<IRelativePointer, WindowsRelativePointer>(),
                Transient<IVirtualKeyboard, WindowsVirtualKeyboard>(),
                Singleton<IKeyMapper, WindowsKeysProvider>(),
                Singleton<IVirtualScreen, WindowsDisplay>(),
                Transient<IUpdater, WindowsUpdater>()
            });
        }
    }
}
