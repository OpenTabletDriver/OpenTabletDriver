using System.Diagnostics;
using Eto;
using Eto.Forms;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.UX.Components;
using OpenTabletDriver.UX.Controls;
using OpenTabletDriver.UX.Windows;

namespace OpenTabletDriver.UX
{
    public sealed class MainForm : DesktopForm
    {
        private readonly App _app;
        private readonly RpcClient<IDriverDaemon> _rpc;
        private readonly Placeholder _placeholder;
        private readonly IControlBuilder _controlBuilder;

        public MainForm(App app, RpcClient<IDriverDaemon> rpc, IControlBuilder controlBuilder, IServiceProvider serviceProvider)
        {
            DataContext = _app = app;
            _rpc = rpc;
            _controlBuilder = controlBuilder;

            Width = 1100;
            Height = 800;

            Content = _placeholder = new Placeholder
            {
                Text = "Connecting to OpenTabletDriver Daemon..."
            };

            TitleBinding.BindDataContext((App a) => a.MainFormTitle);

            var modifier = Application.Instance.CommonModifier;

            var loadSettings = new AppCommand("Load settings...", LoadSettingsDialog, modifier | Keys.O);
            var saveSettings = new AppCommand("Save settings", _app.SaveSettings, modifier | Keys.S);
            var saveSettingsAs = new AppCommand("Save settings as...", SaveSettingsDialog, modifier | Keys.Shift | Keys.S);
            var applySettings = new AppCommand("Apply settings", _app.ApplySettings, modifier | Keys.Enter);
            var discardSettings = new AppCommand("Discard settings", _app.DiscardSettings);
            var resetDefaults = new AppCommand("Reset to defaults", _app.ResetSettings, modifier | Keys.Shift | Keys.R);

            // TODO: Presets menu items
            var presetsMenu = new ButtonMenuItem
            {
                Text = "Presets"
            };

            var fileMenu = new ButtonMenuItem
            {
                Text = "&File",
                Items =
                {
                    loadSettings,
                    saveSettings,
                    saveSettingsAs,
                    applySettings,
                    discardSettings,
                    resetDefaults,
                    new SeparatorMenuItem(),
                    presetsMenu
                }
            };

            var detectTablet = new AppCommand("Detect tablet", DetectTablets, modifier | Keys.D);
            var tabletDebugger = new AppCommand("Tablet debugger...", _app.ShowWindow<TabletDebugger>, modifier | Keys.Shift | Keys.D);
            var configurationEditor = new AppCommand("Configuration editor...", _app.ShowWindow<ConfigurationEditor>, modifier | Keys.Shift | Keys.E);

            var tabletsMenu = new ButtonMenuItem
            {
                Text = "Tablets",
                Items =
                {
                    detectTablet,
                    tabletDebugger,
                    configurationEditor,
                }
            };

            var pluginManager = new AppCommand("Plugin manager...", _app.ShowWindow<PluginManager>, modifier | Keys.P);

            var pluginsMenu = new ButtonMenuItem
            {
                Text = "Plugins",
                Items =
                {
                    pluginManager
                }
            };

            var exportDiagnostics = new AppCommand("Export diagnostics...", ExportDiagnosticsDialog, modifier | Keys.E);
            var showWiki = new AppCommand("OpenTabletDriver wiki...", () => _app.Open(Metadata.WIKI_URL));

            var helpMenu = new ButtonMenuItem
            {
                Text = "&Help",
                Items =
                {
                    exportDiagnostics,
                    showWiki
                }
            };

            var reconnect = new AppCommand("Reconnect to daemon", Reconnect);
            var debugBreak = new AppCommand("Debugger break", Debugger.Break);

            var debugMenu = new ButtonMenuItem
            {
                Text = "Debug",
                Visible = Debugger.IsAttached,
                Items =
                {
                    reconnect,
                    debugBreak
                }
            };

            var quitCommand = new AppCommand("Quit", () => App.Exit(), modifier | Keys.Q);
            var aboutCommand = new AppCommand("About...", () => serviceProvider.GetRequiredService<AboutDialog>().ShowDialog(this), Keys.F1);

            Menu = new MenuBar
            {
                Items =
                {
                    fileMenu,
                    tabletsMenu,
                    pluginsMenu,
                    helpMenu,
                    debugMenu
                },
                QuitItem = quitCommand,
                AboutItem = aboutCommand
            };

            InitializeAsync().Run();
        }

        /// <summary>
        /// Initialize asynchronous components.
        /// </summary>
        private async Task InitializeAsync()
        {
            _rpc.Connected += (_, _) => OnConnected().Run();
            _rpc.Disconnected += (_, _) => Application.Instance.AsyncInvoke(OnDisconnected);

            _app.StartDaemon();
            await _rpc.Connect();
        }

        /// <summary>
        /// The event handler for <see cref="RpcClient{T}.Connected"/>.
        /// This is called when RPC connects to the OpenTabletDriver daemon and builds all dependent UI.
        /// </summary>
        private async Task OnConnected()
        {
            // Synchronize before building the main panel, this will avoid flickering
            await _app.Synchronize();

            await Application.Instance.InvokeAsync(() => Content = _controlBuilder.Build<SettingsPanel>());
        }

        /// <summary>
        /// The event handler for <see cref="RpcClient{T}.Disconnected"/>.
        /// This is called when RPC disconnects from the OpenTabletDriver daemon and returns to the placeholder UI.
        /// </summary>
        private void OnDisconnected()
        {
            Content = _placeholder;
            _app.Desynchronize();

            _rpc.Connect().Run();
        }

        /// <summary>
        /// Prompts for a file to load settings from and applies it if valid.
        /// </summary>
        private async Task LoadSettingsDialog()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Load OpenTabletDriver settings...",
                Directory = new Uri(EtoEnvironment.GetFolderPath(EtoSpecialFolder.Documents)),
                Filters =
                {
                    new FileFilter("Settings (*.json)", ".json")
                }
            };

            if (dialog.ShowDialog(this) == DialogResult.Ok)
                await _app.LoadSettings(dialog.FileName);
        }

        /// <summary>
        /// Prompts for a file to save settings.
        /// </summary>
        private async Task SaveSettingsDialog()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Save OpenTabletDriver settings...",
                Directory = new Uri(EtoEnvironment.GetFolderPath(EtoSpecialFolder.Documents)),
                Filters =
                {
                    new FileFilter("Settings (*.json)", ".json")
                }
            };

            if (dialog.ShowDialog(this) == DialogResult.Ok)
                await _rpc.Instance!.SaveSettings(_app.Settings);
        }

        /// <summary>
        /// Prompts to export a diagnostics file.
        /// </summary>
        private async Task ExportDiagnosticsDialog()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Exporting OpenTabletDriver diagnostics...",
                Directory = new Uri(EtoEnvironment.GetFolderPath(EtoSpecialFolder.Documents)),
                Filters =
                {
                    new FileFilter("Diagnostics (*.txt)", ".txt")
                }
            };

            if (dialog.ShowDialog(this) == DialogResult.Ok)
            {
                var path = dialog.FileName;
                if (Path.GetExtension(path) != ".txt")
                    path = Path.GetFileNameWithoutExtension(path) + ".txt";

                var diagnostics = await _rpc.Instance!.GetDiagnostics();
                Serialization.Serialize(new FileInfo(path), diagnostics);
            }
        }

        /// <summary>
        /// Invokes a device scan for all connected tablets.
        /// </summary>
        private async Task DetectTablets()
        {
            await _rpc.Instance!.DetectTablets();
        }

        /// <summary>
        /// Forces RPC to reconnect.
        /// </summary>
        private async Task Reconnect()
        {
            await _rpc.Reconnect();
        }
    }
}
