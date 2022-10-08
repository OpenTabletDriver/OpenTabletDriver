using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Desktop.Interop.Display;
using OpenTabletDriver.Desktop.Interop.Environment;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Desktop.Interop.Input.Keyboard;
using OpenTabletDriver.Desktop.Interop.Input.Relative;
using OpenTabletDriver.Desktop.Interop.Timer;
using OpenTabletDriver.Desktop.Updater;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Environment;
using OpenTabletDriver.Platform.Keyboard;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop
{
    using static ServiceDescriptor;

    public sealed class DesktopWindowsServiceCollection : DesktopServiceCollection
    {
        private static readonly IEnumerable<ServiceDescriptor> PlatformRequiredServices = new[]
        {
            Transient<IEnvironmentHandler, WindowsEnvironmentHandler>(),
            Transient<EnvironmentDictionary, WindowsEnvironmentDictionary>(),
            Transient<ITimer, WindowsTimer>(),
            Transient<IAbsolutePointer, WindowsAbsolutePointer>(),
            Transient<IRelativePointer, WindowsRelativePointer>(),
            Transient<IVirtualKeyboard, WindowsVirtualKeyboard>(),
            Singleton<IKeysProvider, WindowsKeysProvider>(),
            Transient<IVirtualScreen, WindowsDisplay>(),
            Transient<IUpdater, WindowsUpdater>()
        };

        public DesktopWindowsServiceCollection() : base(PlatformRequiredServices)
        {
        }
    }
}
