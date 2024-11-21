using System.Diagnostics;
using Eto;
using Eto.Forms;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Contracts;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.RPC;
using OpenTabletDriver.Desktop.Updater;
using OpenTabletDriver.UX.Components;
using OpenTabletDriver.UX.Controls;
using OpenTabletDriver.UX.Dialogs;
using OpenTabletDriver.UX.Windows;

namespace OpenTabletDriver.UX
{
    public sealed class MainForm : DesktopForm
    {
        private readonly App _app;
        private readonly RpcClient<IDriverDaemon> _rpc;
        private readonly Placeholder _placeholder;
        private readonly IControlBuilder _controlBuilder;
        private bool _reconnecting;

        public MainForm(App app, RpcClient<IDriverDaemon> rpc, IControlBuilder controlBuilder, IServiceProvider serviceProvider)
        {
            DataContext = _app = app;
            _rpc = rpc;
            _controlBuilder = controlBuilder;

            Width = 1100;
            Height = 800;

            Content = _placeholder = new Placeholder("Connecting to OpenTabletDriver Daemon...");

            TitleBinding.BindDataContext((App a) => a.MainFormTitle);

            var modifier = Application.Instance.CommonModifier;

            var presetsMenu = new ButtonMenuItem
            {
                Text = "Presets",
                Items =
                {
                    new AppCommand("Save preset...", SavePresetDialog),
                    new AppCommand("Open presets directory", () => OpenAppDirectory(a => a.PresetDirectory)),
                    _controlBuilder.Build<PresetsMenuItem>()
                }
            };

            var fileMenu = new ButtonMenuItem
            {
                Text = "&File",
                Items =
                {
                    new AppCommand("Load settings...", LoadSettingsDialog, modifier | Keys.O),
                    new AppCommand("Save settings", _app.SaveSettings, modifier | Keys.S),
                    new AppCommand("Save settings as...", SaveSettingsDialog, modifier | Keys.Shift | Keys.S),
                    new AppCommand("Apply settings", _app.ApplySettings, modifier | Keys.Enter),
                    new AppCommand("Discard settings", _app.DiscardSettings),
                    new AppCommand("Reset to defaults", _app.ResetSettings, modifier | Keys.Shift | Keys.R),
                    new SeparatorMenuItem(),
                    presetsMenu
                }
            };

            var tabletsMenu = new ButtonMenuItem
            {
                Text = "Tablets",
                Items =
                {
                    new AppCommand("Detect tablet", DetectTablets, modifier | Keys.D),
                    new AppCommand("Tablet debugger...", _app.ShowWindow<TabletDebugger>, modifier | Keys.Shift | Keys.D),
                    new AppCommand("Configuration editor...", _app.ShowWindow<ConfigurationEditor>, modifier | Keys.Shift | Keys.E),
                }
            };

            var pluginsMenu = new ButtonMenuItem
            {
                Text = "Plugins",
                Items =
                {
                    new AppCommand("Plugin manager...", _app.ShowWindow<PluginManager>, modifier | Keys.P),
                    new AppCommand("Open plugin directory", () => OpenAppDirectory(a => a.PluginDirectory))
                }
            };

            var helpMenu = new ButtonMenuItem
            {
                Text = "&Help",
                Items =
                {
                    new AppCommand("Export diagnostics...", ExportDiagnosticsDialog, modifier | Keys.E),
                    new AppCommand("OpenTabletDriver wiki...", () => _app.Open(Metadata.WIKI_URL))
                }
            };

            var debugMenu = new ButtonMenuItem
            {
                Text = "Debug",
                Visible = Debugger.IsAttached,
                Items =
                {
                    new AppCommand("Reconnect to daemon", Reconnect),
                    new AppCommand("Minimize", Minimize),
                    new AppCommand("Debugger break", Debugger.Break)
                }
            };

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
                QuitItem = new AppCommand("Quit", app.Exit, modifier | Keys.Q),
                AboutItem = new AppCommand("About...", () => serviceProvider.GetRequiredService<AboutDialog>().ShowDialog(this), Keys.F1)
            };

            InitializeAsync().Run();
        }

        /// <summary>
        /// Initialize asynchronous components.
        /// </summary>
        private async Task InitializeAsync()
        {
            _rpc.Connected += (_, _) => OnConnected().Run();
            _rpc.Disconnected += (_, _) => OnDisconnected().Run();

            _app.StartDaemon();

            try
            {
                await _rpc.Connect();
            }
            catch (TimeoutException)
            {
                _app.ShowDialog<FatalErrorDialog>(this, "Unable to connect to the OpenTabletDriver Daemon.");
                _app.Exit(1);
            }
            catch (Exception ex)
            {
                ex.Show();
                _app.Exit(2);
            }
        }

        /// <summary>
        /// The event handler for <see cref="RpcClient{T}.Connected"/>.
        /// This is called when RPC connects to the OpenTabletDriver daemon and builds all dependent UI.
        /// </summary>
        private async Task OnConnected()
        {
            // Wait a bit before synchronizing, avoids RPC connection errors from an updating state
            await Task.Delay(100);

            // Synchronize before building the main panel, this will avoid flickering
            await _app.Synchronize();
            _rpc.Instance!.Resynchronize += (_, _) => Application.Instance.InvokeAsync(async () => await _app.Synchronize());

            await Application.Instance.InvokeAsync(() => Content = _controlBuilder.Build<SettingsPanel>());

            if (await _rpc.Instance!.CheckForUpdates() is SerializedUpdateInfo updateInfo)
                _app.ShowWindow<UpdateForm>(updateInfo);
        }

        /// <summary>
        /// The event handler for <see cref="RpcClient{T}.Disconnected"/>.
        /// This is called when RPC disconnects from the OpenTabletDriver daemon and returns to the placeholder UI.
        /// </summary>
        private async Task OnDisconnected()
        {
            Content = _placeholder;
            _app.Desynchronize();

            await Application.Instance.InvokeAsync(() =>
            {
                foreach (var window in Application.Instance.Windows.SkipWhile(w => w == this).ToArray())
                    window.Close();

                // We're manually reconnecting, so don't show the error dialog.
                if (_reconnecting)
                {
                    _reconnecting = false;
                    return;
                }

                MessageBox.Show(
                    "Lost connection to daemon. Exiting...",
                    MessageBoxType.Error
                );

                Environment.Exit(1);
            });

            // Only reachable when manually reconnecting
            await _rpc.Connect();
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
        private void SaveSettingsDialog()
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
                Serialization.Serialize(new FileInfo(dialog.FileName), _app.Settings);
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
        /// Prompts to save a preset.
        /// </summary>
        private async Task SavePresetDialog()
        {
            var dialog = new StringDialog
            {
                Title = "Save OpenTabletDriver preset..."
            };

            if (await dialog.ShowModalAsync(this) is string name)
                await _rpc.Instance!.SavePreset(name, _app.Settings);
        }

        /// <summary>
        /// Opens a directory in the preferred file manager.
        /// </summary>
        /// <param name="getMember">A function pointing to the application directory.</param>
        private async Task OpenAppDirectory(Func<IAppInfo, string> getMember)
        {
            var appInfo = await _rpc.Instance!.GetApplicationInfo();

            var path = getMember(appInfo);
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            _app.Open(path, true);
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
        private void Reconnect()
        {
            _reconnecting = true;
            _rpc.Disconnect();
        }
    }
}
