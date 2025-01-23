using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.UI.Navigation;
using OpenTabletDriver.UI.ViewModels;
using OpenTabletDriver.UI.Views;

namespace OpenTabletDriver.UI;

public static class AppRoutes
{
    // Navigation hosts
    public const string MainHost = "MainHost";

    // Routes
    public const string DaemonConnectionRoute = "DaemonConnection";
    public const string TabletsOverviewRoute = "TabletsOverview";
    public const string TabletMainSettingsRoute = "TabletMainSettings";
    public const string TabletBindingsRoute = "TabletBindings";
    public const string TabletFiltersRoute = "TabletFilters";
    public const string ToolsSettingsRoute = "ToolsSettings";
    public const string PresetsManagerRoute = "PresetsManager";
    public const string PluginManagerRoute = "PluginManager";
    public const string DiagnosticsRoute = "Diagnostics";
    public const string SettingsRoute = "Settings";

    public static IServiceCollection AddApplicationViewModels(this IServiceCollection services)
    {
        return services
            .AddTransient<MainWindowViewModel>()
            .AddTransient<NavigationPaneViewModel>()
            .AddTransient<BindingMenuViewModel>();
    }

    public static IServiceCollection AddApplicationRoutes(this IServiceCollection services)
    {
        return services
            .AddNavigationMapping<NavigationMapNotFoundViewModel, NavigationMapNotFoundView>()
            .AddSingletonRoute<DaemonConnectionViewModel, DaemonConnectionView>(DaemonConnectionRoute)
            .AddSingletonRoute<UISettingsViewModel, UISettingsView>(SettingsRoute) // register as singleton to preserve "modified" state
            .AddSingletonRoute<TabletsOverviewViewModel, TabletsOverview>(TabletsOverviewRoute) // preserve tablet selection
            .AddNavigationMapping<TabletViewModel, TabletView>();
        // .AddNavigationRoute<TabletMainSettingsViewModel, TabletMainSettingsView>(TabletMainSettingsRoute)
        // .AddNavigationRoute<TabletBindingsViewModel, TabletBindingsView>(TabletBindingsRoute)
        // .AddNavigationRoute<TabletFiltersViewModel, TabletFiltersView>(TabletFiltersRoute)
        // .AddNavigationRoute<PluginsSettingsViewModel, PluginsSettingsView>(PluginsSettingsRoute)
        // .AddNavigationRoute<PluginManagerViewModel, PluginManagerView>(PluginManagerRoute)
        // .AddNavigationRoute<DiagnosticsViewModel, DiagnosticsView>(DiagnosticsRoute)
    }
}
