using Microsoft.Extensions.DependencyInjection;

namespace OpenTabletDriver.UI.Services.Windows;

public static class ServiceExtensions
{
    public static IServiceCollection AddWindowsServices(this IServiceCollection services)
    {
        return services
            .AddSingleton<IAutoStartService, ShortcutAutoStartService>();
    }
}
