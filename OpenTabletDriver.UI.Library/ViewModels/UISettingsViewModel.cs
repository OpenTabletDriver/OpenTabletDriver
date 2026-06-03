using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenTabletDriver.UI.Models;
using OpenTabletDriver.UI.Services;

namespace OpenTabletDriver.UI.ViewModels;

public partial class UISettingsViewModel : ActivatableViewModelBase
{
    private readonly IUISettingsProvider _settingsProvider;
    private readonly IAutoStartService _autoStartService;
    private readonly IDriverDaemonAutoStartService _driverDaemonAutoStartService;

    [ObservableProperty]
    private UISettings? _settings;

    [ObservableProperty]
    private string? _autoStartLabel;

    [ObservableProperty]
    private bool _autoStart;

    [ObservableProperty]
    private string? _driverDaemonAutoStartLabel;

    [ObservableProperty]
    private bool _driverDaemonAutoStart;

    [ObservableProperty]
    private bool _driverDaemonAutoStartHideWindowVisible;

    [ObservableProperty]
    private bool _driverDaemonAutoStartHideWindow;

    private bool _modified;

    public bool Modified
    {
        get => _modified;
        private set
        {
            SetProperty(ref _modified, value);
            SaveSettingsCommand.NotifyCanExecuteChanged();
        }
    }

    public UISettingsViewModel(
        IUISettingsProvider settingsProvider,
        IAutoStartService autoStartService,
        IDriverDaemonAutoStartService driverDaemonAutoStartService
    )
    {
        _settingsProvider = settingsProvider;
        _autoStartService = autoStartService;
        _driverDaemonAutoStartService = driverDaemonAutoStartService;

        WhenActivated(d =>
        {
            var modified = Modified; // Preserve modified state
            _settingsProvider.WhenLoadedOrSet(
                onLoad: (provider, settings) =>
                {
                    if (settings != Settings)
                    {
                        Settings = settings;
                        if (settings is not null)
                            settings.PropertyChanged += HandleSettingsChanged;
                    }
                },
                onException: (provider, exception) =>
                {
                    if (exception is not null)
                    {
                        // App.NotifyException(exception)
                        _settingsProvider.Settings = new UISettings();
                    }
                }
            ).DisposeWith(d);

            AutoStart = _autoStartService.AutoStart;
            DriverDaemonAutoStart = _driverDaemonAutoStartService.AutoStart;
            DriverDaemonAutoStartHideWindow = _driverDaemonAutoStartService.HideWindow;
            DriverDaemonAutoStartHideWindowVisible = _driverDaemonAutoStartService.HideWindowSupported;

            // Maybe convert to a drop-down to select auto-start backend?
            AutoStartLabel = !string.IsNullOrEmpty(_autoStartService.BackendName)
                ? "Open UI on login with " + _autoStartService.BackendName
                : null;
            DriverDaemonAutoStartLabel = !string.IsNullOrEmpty(_driverDaemonAutoStartService.BackendName)
                ? "Start daemon on login with " + _driverDaemonAutoStartService.BackendName
                : null;

            Modified = modified;
        });
    }

    private void HandleSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        Modified = true;
    }

    [RelayCommand(CanExecute = nameof(IsModified))]
    private async Task SaveSettingsAsync()
    {
        _settingsProvider.Settings = Settings;
        await _settingsProvider.SaveSettingsAsync();

        if (!_autoStartService.TrySetAutoStart(AutoStart))
        {
            // TODO: notify failure
            AutoStart = _autoStartService.AutoStart;
        }

        if (!_driverDaemonAutoStartService.TrySetAutoStart(DriverDaemonAutoStart, DriverDaemonAutoStartHideWindow))
        {
            // TODO: notify failure
            DriverDaemonAutoStart = _driverDaemonAutoStartService.AutoStart;
            DriverDaemonAutoStartHideWindow = _driverDaemonAutoStartService.HideWindow;
        }

        Modified = false;
    }

    [RelayCommand]
    private void ResetSettings()
    {
        var firstLaunch = true;

        var oldSettings = Settings;
        if (oldSettings != null)
        {
            firstLaunch = oldSettings.FirstLaunch;
            oldSettings.PropertyChanged -= HandleSettingsChanged;
        }

        _settingsProvider.Settings = new UISettings()
        {
            FirstLaunch = firstLaunch
        };
    }

    private bool IsModified() => Modified;
    partial void OnAutoStartChanging(bool value) => Modified = true;
    partial void OnDriverDaemonAutoStartChanging(bool value) => Modified = true;
    partial void OnDriverDaemonAutoStartHideWindowChanging(bool value) => Modified = true;
}
