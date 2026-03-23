using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Library.Diagnostics;
using OpenTabletDriver.Daemon.Library.Interop.Display;
using OpenTabletDriver.Daemon.Library.Interop.Environment;
using OpenTabletDriver.Daemon.Library.Interop.Input.Absolute;
using OpenTabletDriver.Daemon.Library.Interop.Input.Keyboard;
using OpenTabletDriver.Daemon.Library.Interop.Input.Relative;
using OpenTabletDriver.Daemon.Library.Interop.Timer;
using OpenTabletDriver.Daemon.Library.Updater;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Environment;
using OpenTabletDriver.Platform.Keyboard;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Library.Interop
{
    using static ServiceDescriptor;

    public sealed class DesktopWindowsServiceCollection : DesktopServiceCollection
    {
        public DesktopWindowsServiceCollection() : base()
        {
            this.AddServices(new[] {
                Transient<IEnvironmentHandler, WindowsEnvironmentHandler>(),
                Transient<IEnvironmentDictionary, WindowsEnvironmentDictionary>(),
                Transient<ITimer, WindowsTimer>(),
                Transient<IAbsolutePointer, WindowsAbsolutePointer>(),
                Transient<IRelativePointer, WindowsRelativePointer>(),
                Transient<IVirtualKeyboard, WindowsVirtualKeyboard>(),
                Singleton<IKeyMapper, WindowsKeysProvider>(),
                Singleton<IVirtualScreen, WindowsDisplay>(),
                Transient<IUpdater, WindowsUpdater>(),
                Singleton<IAppInfo, WindowsAppInfo>(),
            });
        }
    }
}
