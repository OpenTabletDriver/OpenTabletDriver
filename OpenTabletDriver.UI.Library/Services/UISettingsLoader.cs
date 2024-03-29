using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.UI.Services;

namespace OpenTabletDriver.UI;

public class UISettingsLoader : IStartupJob
{
    private readonly IServiceProvider _serviceProvider;

    public UISettingsLoader(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Run()
    {
        // construct the provider, this will load the settings on application startup
        _serviceProvider.GetRequiredService<IUISettingsProvider>();
    }
}
