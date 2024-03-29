using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Daemon.Diagnostics;
using OpenTabletDriver.Daemon.Interop.Display;
using OpenTabletDriver.Daemon.Interop.Environment;
using OpenTabletDriver.Daemon.Interop.Input.Absolute;
using OpenTabletDriver.Daemon.Interop.Input.Keyboard;
using OpenTabletDriver.Daemon.Interop.Input.Relative;
using OpenTabletDriver.Daemon.Interop.Timer;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Environment;
using OpenTabletDriver.Platform.Keyboard;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Interop
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
                Transient<IAbsolutePointer, EvdevAbsolutePointer>(),
                Transient<IRelativePointer, EvdevRelativePointer>(),
                Transient<IPressureHandler, EvdevVirtualTablet>(),
                Transient<IVirtualKeyboard, EvdevVirtualKeyboard>(),
                Singleton<IKeyMapper, LinuxKeysProvider>(),
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
