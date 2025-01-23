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

    [ObservableProperty]
    private UISettings? _settings;

    [ObservableProperty]
    private string? _autoStartLabel;

    [ObservableProperty]
    private bool _autoStart;

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

    public UISettingsViewModel(IUISettingsProvider settingsProvider, IAutoStartService autoStartService)
    {
        _settingsProvider = settingsProvider;
        _autoStartService = autoStartService;

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

            // Maybe convert to a drop-down to select auto-start backend?
            AutoStartLabel = !string.IsNullOrEmpty(_autoStartService.BackendName)
                ? "Auto-start with " + _autoStartService.BackendName
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
}
