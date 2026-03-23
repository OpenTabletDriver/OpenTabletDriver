using Avalonia;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTabletDriver.UI;

public static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        RunAvaloniaApp(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure(() => SetupOpenTabletDriverApp(Array.Empty<string>()))
            .UsePlatformDetect()
            .LogToTrace();

    private static void RunAvaloniaApp(string[] args)
    {
        AppBuilder.Configure(() => SetupOpenTabletDriverApp(args))
            .UsePlatformDetect()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(args);
    }

    private static App SetupOpenTabletDriverApp(string[] args)
    {
        var serviceProvider = new ServiceCollection()
            .WithDefaultServices()
            .WithPlatformServices()
            .WithUIEnvironmentFrom(args)
            .AddApplicationViewModels()
            .AddApplicationRoutes()
            .BuildServiceProvider();

        return serviceProvider.GetRequiredService<App>();
    }
}
