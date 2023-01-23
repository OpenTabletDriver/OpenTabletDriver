using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Eto.Forms;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.UX.Models;

namespace OpenTabletDriver.UX.ViewModels
{
    /// <summary>
    /// Represents the main view model and is responsible for
    /// managing the daemon connection and the tablet and tool view models.
    /// </summary>
    public sealed partial class MainWindowViewModel : BaseViewModel
    {
        private readonly DaemonService _daemonService = new();

        /// <summary>
        /// Gets or sets a boolean indicating whether the daemon is connected.
        /// </summary>
        [ObservableProperty]
        private bool _isConnected;

        /// <summary>
        /// Gets or sets an array of currently available displays.
        /// </summary>
        [ObservableProperty]
        private ImmutableArray<DisplayDto> _displays;

        /// <summary>
        /// Gets or sets an array of the settings currently applied for each tool.
        /// </summary>
        [ObservableProperty]
        private ImmutableArray<PluginSettings> _toolSettings;

        /// <summary>
        /// Gets or sets an array that contains information about the plugins currently loaded and ways to configure them.
        /// </summary>
        [ObservableProperty]
        private ImmutableArray<PluginContextDto> _plugins;

        /// <summary>
        /// Gets an observable collection of tablet view models.
        /// </summary>
        public ObservableCollection<TabletViewModel> Tablets { get; } = new ObservableCollection<TabletViewModel>();

        /// <summary>
        /// Gets an observable collection of tool view models.
        /// </summary>
        public ObservableCollection<ToolViewModel> Tools { get; } = new ObservableCollection<ToolViewModel>();

        /// <summary>
        /// Gets an observable collection of preset names.
        /// </summary>
        public ObservableCollection<string> Presets { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets an asynchronous command that saves the current settings.
        /// </summary>
        public IAsyncRelayCommand SaveSettingsCommand { get; }

        /// <summary>
        /// Gets an asynchronous command that resets the current settings.
        /// </summary>
        public IAsyncRelayCommand ResetSettingsCommand { get; }

        /// <summary>
        /// Gets an asynchronous command that gets the current presets. This updates the <see cref="Presets"/> collection.
        /// </summary>
        public IAsyncRelayCommand GetPresetsCommand { get; }

        /// <summary>
        /// Gets an asynchronous command that applies a preset.
        /// </summary>
        public IAsyncRelayCommand<string> ApplyPresetCommand { get; }

        /// <summary>
        /// Gets an asynchronous command that saves the currently applied settings as a preset.
        /// </summary>
        public IAsyncRelayCommand<string> SaveAsPresetCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// Requires <see cref="InitializeAsync"/> to be called after construction before use.
        /// </remarks>
        public MainWindowViewModel()
        {
            _daemonService.PropertyChanged += (sender, e) => Application.Instance.AsyncInvoke(() =>
            {
                if (e.PropertyName == nameof(_daemonService.IsConnected))
                    IsConnected = _daemonService.IsConnected;
            });

            SaveSettingsCommand = new AsyncRelayCommand(SaveSettingsAsync, canExecute: () => IsConnected);
            ResetSettingsCommand = new AsyncRelayCommand(ResetSettingsAsync, canExecute: () => IsConnected);
            GetPresetsCommand = new AsyncRelayCommand(GetPresetsAsync, canExecute: () => IsConnected);
            ApplyPresetCommand = new AsyncRelayCommand<string>(ApplyPresetAsync, canExecute: ApplyPresetValidate);
            SaveAsPresetCommand = new AsyncRelayCommand<string>(SaveAsPresetAsync, canExecute: _ => IsConnected);

            Task.Run(_daemonService.ConnectAsync).ConfigureAwait(false);
        }

        /// <summary>
        /// Initializes the view model by subscribing to events and getting the initial data.
        /// </summary>
        /// <returns>An awaitable task that completes when all setup is complete.</returns>
        private async Task InitializeAsync()
        {
            _daemonService.Instance.TabletAdded += Daemon_TabletAdded;
            _daemonService.Instance.TabletRemoved += Daemon_TabletRemoved;
            _daemonService.Instance.ToolsChanged += Daemon_ToolsChanged;
            Displays = (await _daemonService.Instance.GetDisplays()).ToImmutableArray();
            ToolSettings = (await _daemonService.Instance.GetToolSettings()).ToImmutableArray();
            Plugins = (await _daemonService.Instance.GetPlugins()).ToImmutableArray();
            await SetupTabletViewModels();
            await SetupToolViewModels();
            await GetPresetsAsync();
        }

        /// <summary>
        /// Saves the currently applied settings.
        /// </summary>
        /// <returns>An awaitable task that completes when daemon is done saving settings.</returns>
        private async Task SaveSettingsAsync()
        {
            // TODO: apply profiles of tablets with dirty profiles.
            // TODO: apply settings of tools with dirty settings.
            await _daemonService.Instance.SaveSettings();
        }

        /// <summary>
        /// Resets the settings to its defaults.
        /// </summary>
        /// <returns>An awaitable task that completes when daemon is done resetting settings.</returns>
        private async Task ResetSettingsAsync()
        {
            await _daemonService.Instance.ResetSettings();
        }

        /// <summary>
        /// Gets the current presets. This updates the <see cref="Presets"/> collection.
        /// </summary>
        /// <returns>An awaitable task that completes when <see cref="Presets"/> is updated.</returns>
        private async Task GetPresetsAsync()
        {
            // TODO: switch to using immutable array for presets?
            Presets.Clear();
            foreach (var preset in await _daemonService.Instance.GetPresets())
                Presets.Add(preset);
        }

        /// <summary>
        /// Applies a preset.
        /// </summary>
        /// <param name="preset">The name of the preset to apply.</param>
        /// <returns>An awaitable task that completes when daemon is done applying the preset.</returns>
        private async Task ApplyPresetAsync(string? preset)
        {
            Debug.Assert(preset != null);
            await _daemonService.Instance.ApplyPreset(preset);
        }

        /// <summary>
        /// Validates the preset name for <see cref="ApplyPresetCommand"/>.
        /// </summary>
        /// <param name="preset">The name of the preset to apply.</param>
        /// <returns>True if the preset name is valid and is in the list of known presets, otherwise false.</returns>
        private bool ApplyPresetValidate(string? preset)
        {
            return IsConnected && preset != null && Presets.Contains(preset);
        }

        /// <summary>
        /// Saves the currently applied settings as a preset.
        /// </summary>
        /// <param name="preset">The name of the preset to save.</param>
        /// <returns>An awaitable task that completes when daemon is done saving the preset.</returns>
        private async Task SaveAsPresetAsync(string? preset)
        {
            Debug.Assert(preset != null);
            await _daemonService.Instance.SaveAsPreset(preset);
        }

        private async Task SetupTabletViewModels()
        {
            foreach (var tabletId in await _daemonService.Instance.GetTablets())
            {
                await AddTabletViewModel(tabletId);
            }
        }

        private async Task SetupToolViewModels()
        {
            foreach (var tool in await _daemonService.Instance.GetToolSettings())
            {
                AddToolViewModel(tool);
            }
        }

        private async Task AddTabletViewModel(int tabletId)
        {
            var tabletHandler = await TabletHandler.CreateAsync(_daemonService.Instance, tabletId);
            var tabletViewModel = new TabletViewModel(tabletHandler);
            Tablets.Add(tabletViewModel);
        }

        private void AddToolViewModel(PluginSettings tool)
        {
            var toolDescriptor = _plugins
                .SelectMany(c => c.Plugins)
                .FirstOrDefault(p => p.Path == tool.Path);

            var toolViewModel = new ToolViewModel(toolDescriptor, tool);
            Tools.Add(toolViewModel);
        }

        private void Daemon_TabletAdded(object? sender, int tabletId)
        {
            AddTabletViewModel(tabletId).ConfigureAwait(false);
        }

        private void Daemon_TabletRemoved(object? sender, int tabletId)
        {
            for (int i = 0; i < Tablets.Count; i++)
            {
                if (Tablets[i].TabletId == tabletId)
                {
                    Tablets.RemoveAt(i);
                    return;
                }
            }
        }

        private void Daemon_ToolsChanged(object? sender, IEnumerable<PluginSettings> tools)
        {
            foreach (var newToolSetting in tools)
            {
                var tool = Tools.FirstOrDefault(t => t.Settings.Path == newToolSetting.Path);
                if (tool is null)
                    continue;

                tool.Settings = newToolSetting;
            }
        }

        partial void OnIsConnectedChanged(bool value)
        {
            if (value)
            {
                InitializeAsync().ConfigureAwait(false);
            }
            else
            {
                Displays = ImmutableArray<DisplayDto>.Empty;
                ToolSettings = ImmutableArray<PluginSettings>.Empty;
                Plugins = ImmutableArray<PluginContextDto>.Empty;
                Tablets.Clear();
                Tools.Clear();
                Presets.Clear();

                Task.Run(_daemonService.ConnectAsync).ConfigureAwait(false);
            }
        }
    }
}
