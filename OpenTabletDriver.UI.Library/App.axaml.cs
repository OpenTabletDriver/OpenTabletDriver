using System.Runtime.ExceptionServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using OpenTabletDriver.UI.Views;

namespace OpenTabletDriver.UI;

public class App : Application
{
    private IEnumerable<IStartupJob>? _startupJobs;

    public App(IServiceProvider provider, IEnumerable<IStartupJob> startupJobs)
    {
        Ioc.Default.ConfigureServices(provider); // allow use of Ioc.Default for DI
        _startupJobs = startupJobs;

        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            ExceptionDispatchInfo.Throw(e.Exception); // forcibly crash
        };
    }

    // TODO: designer support
    public App()
    {
        if (!Design.IsDesignMode)
            throw new InvalidOperationException("This constructor is for designer use only");
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        RemoveAvaloniaValidationPlugin();
        RunStartupJobs();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindowView();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void RunStartupJobs()
    {
        if (_startupJobs is null)
            return;

        foreach (var job in _startupJobs)
            job.Run();

        _startupJobs = null;
    }

    private static void RemoveAvaloniaValidationPlugin()
    {
        var dataValidators = BindingPlugins.DataValidators;
        for (int i = 0; i < dataValidators.Count; i++)
            if (dataValidators[i] is DataAnnotationsValidationPlugin)
                dataValidators.RemoveAt(i--);
    }
}
