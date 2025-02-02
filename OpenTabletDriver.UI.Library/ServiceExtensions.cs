using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Contracts.RPC;
using OpenTabletDriver.UI.Models;
using OpenTabletDriver.UI.Navigation;
using OpenTabletDriver.UI.Services;
using OpenTabletDriver.UI.Services.Windows;

namespace OpenTabletDriver.UI;

public static class ServiceExtensions
{
    public static IServiceCollection AddStartupJob<T>(this IServiceCollection services)
        where T : class, IStartupJob
    {
        return services.AddTransient<IStartupJob, T>();
    }

    public static IServiceCollection WithDefaultServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<App>()
            .AddSingleton<IRpcClient<IDriverDaemon>>(_ => new RpcClient<IDriverDaemon>("OpenTabletDriver.Daemon"))
            .AddSingleton<IDaemonService, DaemonService>()
            .AddSingleton<IDispatcher>(_ => Dispatcher.UIThread)
            .AddSingleton<IUISettingsProvider, UISettingsProvider>()
            .UseNavigation<NavigatorFactory>()
            .AddSingleton<IMessenger, StrongReferenceMessenger>()
            .AddSingleton<IAutoStartService, NullAutoStartService>();
    }

    public static IServiceCollection WithUIEnvironmentFrom(this IServiceCollection services, string[] args)
    {
        return services.AddSingleton(UIEnvironment.Create(args));
    }

    public static IServiceCollection WithPlatformServices(this IServiceCollection services)
    {
        if (OperatingSystem.IsWindows())
            return services.AddWindowsServices();
        // else if (OperatingSystem.IsLinux())
        //     return services.AddLinuxServices();
        // else if (OperatingSystem.IsMacOS())
        //     return services.AddMacServices();
        else
            return services;
    }
}
