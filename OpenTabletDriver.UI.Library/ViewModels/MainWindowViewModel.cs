using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OpenTabletDriver.UI.Messages;
using OpenTabletDriver.UI.Navigation;
using OpenTabletDriver.UI.Services;

namespace OpenTabletDriver.UI.ViewModels;

/// <summary>
/// Represents the main view model and is responsible for
/// managing the daemon connection and the tablet and tool view models.
/// </summary>
public sealed partial class MainWindowViewModel : WindowViewModelBase
{
    private readonly IDaemonService _daemonService;
    private readonly IDispatcher _dispatcher;
    private readonly IUISettingsProvider _uiSettingsProvider;
    private readonly INavigator _navigator;
    private readonly IMessenger _messenger;

    [ObservableProperty]
    private string _title;

    /// <summary>
    /// Gets or sets a boolean indicating whether the daemon is connected.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveSettingsCommand),
                                nameof(ResetSettingsCommand),
                                nameof(SaveAsPresetCommand))]
    private bool _isConnected;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoBackCommand))]
    private bool _canGoBack;

    [ObservableProperty]
    private bool _isSettingsLoaded;

    public string Version { get; } = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    public MainWindowViewModel(
        IDaemonService daemonService,
        INavigatorFactory navigatorFactory,
        IDispatcher dispatcher,
        IUISettingsProvider uiSettingsProvider,
        IMessenger messenger)
            : base(uiSettingsProvider)
    {
        _title = "OpenTabletDriver v" + Version;
        _daemonService = daemonService;
        _navigator = navigatorFactory.GetOrCreate(AppRoutes.MainHost);
        _dispatcher = dispatcher;
        _uiSettingsProvider = uiSettingsProvider;
        _messenger = messenger;

        _uiSettingsProvider.WhenLoadedOrSet((d, settings) =>
        {
            _dispatcher.Post(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1)); // because we're too fast
                IsSettingsLoaded = true;
                d.Dispose(); // fire once
            });
        });

        _navigator.Navigated += (_, e) => CanGoBack = _navigator.CanGoBack;
        _messenger.Send(new NavigationPaneSelectionChangeRequest(NavigationItemSelection.Daemon));

        _daemonService.HandleProperty(
            nameof(IDaemonService.State),
            d => d.State,
            DaemonService_State_Handler
        );

        _dispatcher.Post(async () =>
        {
            try
            {
                // Connection-time setup is done on OnIsConnectedChanged()
                await _daemonService!.ConnectAsync();
            }
            catch
            {
                // TODO: log
            }
        }, DispatcherPriority.MaxValue);
    }

    private void DaemonService_State_Handler(IDaemonService s, DaemonState state)
    {
        _dispatcher.ProbablySynchronousPost(() =>
        {
            IsConnected = _daemonService.State == DaemonState.Connected;
        });
    }

    /// <summary>
    /// Initializes the view model by subscribing to events and getting the initial data.
    /// </summary>
    /// <returns>An awaitable task that completes when all setup is complete.</returns>
    private async Task InitializeAsync()
    {
        var hasNavigated = false;
        void navigated(object? sender, NavigationEventData e)
        {
            hasNavigated = true;
        }

        _navigator.Navigated += navigated;
        await Task.Delay(TimeSpan.FromSeconds(1));
        _navigator.Navigated -= navigated;

        if (!hasNavigated)
        {
            _messenger.Send(new NavigationPaneSelectionChangeRequest(NavigationItemSelection.Tablet));
        }
    }

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack()
    {
        _navigator.Pop();
    }

    /// <summary>
    /// Saves the currently applied settings.
    /// </summary>
    /// <returns>An awaitable task that completes when daemon is done saving settings.</returns>
    [RelayCommand(CanExecute = nameof(IsConnected))]
    private async Task SaveSettingsAsync()
    {
        // TODO: apply profiles of tablets with dirty profiles.
        // TODO: apply settings of tools with dirty settings.
        Debug.Assert(_daemonService.Instance != null);
        await _daemonService.Instance.SaveSettings();
    }

    /// <summary>
    /// Resets the settings to its defaults.
    /// </summary>
    /// <returns>An awaitable task that completes when daemon is done resetting settings.</returns>
    [RelayCommand(CanExecute = nameof(IsConnected))]
    private async Task ResetSettingsAsync()
    {
        Debug.Assert(_daemonService.Instance != null);
        await _daemonService.Instance.ResetSettings();
    }

    /// <summary>
    /// Saves the currently applied settings as a preset.
    /// </summary>
    /// <param name="preset">The name of the preset to save.</param>
    /// <returns>An awaitable task that completes when daemon is done saving the preset.</returns>
    [RelayCommand(CanExecute = nameof(IsConnected))]
    private async Task SaveAsPresetAsync(string? preset)
    {
        Debug.Assert(preset != null);
        Debug.Assert(_daemonService.Instance != null);
        await _daemonService.Instance.SaveAsPreset(preset);
    }

    [RelayCommand]
    private void NavigateTo(NavigationItemSelection navigationItem)
    {
        _messenger.Send(new NavigationPaneSelectionChangeRequest(navigationItem));
    }

    partial void OnIsConnectedChanged(bool value)
    {
        if (value)
        {
            InitializeAsync().ConfigureAwait(false);
        }
        else
        {
            _messenger.Send(new NavigationPaneSelectionChangeRequest(NavigationItemSelection.Daemon));
        }
    }
}

public class MenuItemViewModel
{
    public string? Header { get; init; }
    public ICommand? Command { get; init; }
    public object? CommandParameter { get; init; }
    public Collection<MenuItemViewModel> Items { get; } = new();
}
