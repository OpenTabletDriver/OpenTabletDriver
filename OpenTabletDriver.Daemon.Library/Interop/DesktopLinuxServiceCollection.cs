using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Library.Diagnostics;
using OpenTabletDriver.Daemon.Library.Interop.Display;
using OpenTabletDriver.Daemon.Library.Interop.Environment;
using OpenTabletDriver.Daemon.Library.Interop.Input.Absolute;
using OpenTabletDriver.Daemon.Library.Interop.Input.Keyboard;
using OpenTabletDriver.Daemon.Library.Interop.Input.Relative;
using OpenTabletDriver.Daemon.Library.Interop.Timer;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Environment;
using OpenTabletDriver.Platform.Keyboard;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Daemon.Library.Interop
{
    using static ServiceDescriptor;

    public sealed class DesktopLinuxServiceCollection : DesktopServiceCollection
    {
        public DesktopLinuxServiceCollection() : base()
        {
            this.AddServices(new[]
            {
                Transient<IEnvironmentHandler, LinuxEnvironmentHandler>(),
                Transient<IEnvironmentDictionary, LinuxEnvironmentDictionary>(),
                Transient(GetTimer),
                Transient<IAbsolutePointer, EvdevAbsolutePointer>(),
                Transient<IRelativePointer, EvdevRelativePointer>(),
                Transient<IPressureHandler, EvdevVirtualTablet>(),
                Transient<IVirtualKeyboard, EvdevVirtualKeyboard>(),
                Singleton<IKeyMapper, LinuxKeysProvider>(),
                GetVirtualScreen(),
                Singleton<IAppInfo, LinuxAppInfo>(),
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
