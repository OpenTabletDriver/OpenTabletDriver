using Microsoft.Extensions.DependencyInjection;
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
