using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Desktop.Interop.Display;
using OpenTabletDriver.Desktop.Interop.Environment;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Desktop.Interop.Input.Keyboard;
using OpenTabletDriver.Desktop.Interop.Input.Relative;
using OpenTabletDriver.Desktop.Interop.Timer;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Environment;
using OpenTabletDriver.Platform.Keyboard;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop
{
    using static ServiceDescriptor;

    public sealed class DesktopLinuxServiceCollection : DesktopServiceCollection
    {
        public DesktopLinuxServiceCollection() : base()
        {
            this.AddServices(new[]
            {
                Transient<IEnvironmentHandler, LinuxEnvironmentHandler>(),
                Transient<EnvironmentDictionary, LinuxEnvironmentDictionary>(),
                Transient(GetTimer),
                Singleton<IAbsolutePointer, EvdevAbsolutePointer>(),
                Singleton<IRelativePointer, EvdevRelativePointer>(),
                Singleton<IPressureHandler, EvdevVirtualTablet>(),
                Singleton<IVirtualKeyboard, EvdevVirtualKeyboard>(),
                Singleton<IKeysProvider, LinuxKeysProvider>(),
                GetVirtualScreen()
            });
        }

        private static ServiceDescriptor GetVirtualScreen()
        {
            if (HasEnvironmentVariable("WAYLAND_DISPLAY"))
                return Singleton<IVirtualScreen, WaylandDisplay>();
            if (HasEnvironmentVariable("DISPLAY"))
                return Singleton<IVirtualScreen, XScreen>();

            throw new InvalidOperationException("Neither Wayland nor X11 environment variables were set");
        }

        private static ITimer GetTimer(IServiceProvider provider)
        {
            if (Debugger.IsAttached)
                return new FallbackTimer();

            return new LinuxTimer();
        }

        private static bool HasEnvironmentVariable(string variable)
        {
            return !string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable(variable));
        }
    }
}
