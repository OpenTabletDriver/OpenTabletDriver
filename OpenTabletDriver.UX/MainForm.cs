using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using Newtonsoft.Json.Linq;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Logging;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls;

namespace OpenTabletDriver.UX
{
    using static App;

    public class MainForm : DesktopForm
    {
        public MainForm()
            : base()
        {
            this.DataContext = App.Current;

            // Call InitializeForm on ctor since DesktopForm.Show() won't be called on binary launch
            InitializeForm();
            InitializePlatform();

            SetTitle();
            Menu = ConstructMenu();

            base.Content = placeholder = new Placeholder
            {
                Text = "Connecting to OpenTabletDriver Daemon..."
            };

            trayIcon?.Indicator?.Show();

            Driver.Connected += HandleDaemonConnected;
            Driver.Disconnected += HandleDaemonDisconnected;

            Application.Instance.AsyncInvoke(async () =>
            {
                try
                {
                    await Driver.Connect();
                    await CheckForUpdates();
                }
                catch (TimeoutException)
                {
                    MessageBox.Show("Daemon connection timed out after some time. Verify that the daemon is running.", "Daemon Connection Timed Out");
                    Application.Instance.Quit();
                }
            });
        }

        private const int DEFAULT_CLIENT_WIDTH = 960;
        private const int DEFAULT_CLIENT_HEIGHT = 760;

        private TabletSwitcherPanel mainPanel;
        private MenuBar menu;
        private Placeholder placeholder;
        private TrayIcon trayIcon;

        protected override void InitializeForm()
        {
            var bounds = Screen.FromPoint(Mouse.Position).Bounds;

            if (this.WindowState != WindowState.Maximized)
            {
                var minWidth = Math.Min(DEFAULT_CLIENT_WIDTH, bounds.Width * 0.95);
                var minHeight = Math.Min(DEFAULT_CLIENT_HEIGHT, bounds.Height * 0.95);

                this.Size = new Size((int)minWidth, (int)minHeight);

                if (DesktopInterop.CurrentPlatform == PluginPlatform.Windows)
                {
                    var x = Screen.WorkingArea.Center.X - (minWidth / 2);
                    var y = Screen.WorkingArea.Center.Y - (minHeight / 2);
                    this.Location = new Point((int)x, (int)y);
                }
            }
        }

        protected void InitializePlatform()
        {
            switch (DesktopInterop.CurrentPlatform)
            {
                case PluginPlatform.MacOS:
                    this.Padding = 10;
                    break;
            }

            bool enableDaemonWatchdog = DesktopInterop.CurrentPlatform switch
            {
                PluginPlatform.Windows => true,
                PluginPlatform.MacOS => true,
                _ => false,
            };

            if (App.EnableTrayIcon)
            {
                trayIcon = new TrayIcon(this);
                if (WindowState == WindowState.Minimized)
                {
                    this.Visible = false;
                    this.ShowInTaskbar = false;
                }
                this.WindowStateChanged += (sender, e) =>
                {
                    switch (this.WindowState)
                    {
                        case WindowState.Normal:
                        case WindowState.Maximized:
                            this.Visible = true;
                            this.ShowInTaskbar = true;
                            break;
                        case WindowState.Minimized:
                            this.Visible = false;
                            this.ShowInTaskbar = false;
                            break;
                    }
                };
                Application.Instance.Terminating += (sender, e) => trayIcon.Dispose();
            }

            if (App.EnableDaemonWatchdog)
            {
                // Check if daemon is already active, if not then start it as a subprocess if it exists in the local path.
                if (!Instance.Exists("OpenTabletDriver.Daemon") && DaemonWatchdog.CanExecute)
                {
                    var watchdog = new DaemonWatchdog();
                    watchdog.Start();
                    watchdog.DaemonExited += (sender, e) =>
                    {
                        Application.Instance.AsyncInvoke(() =>
                        {
                            var dialogResult = MessageBox.Show(
                                this,
                                "Fatal: The OpenTabletDriver Daemon has exited. Do you want to restart it?",
                                "OpenTabletDriver Fatal Error",
                                MessageBoxButtons.YesNo,
                                MessageBoxType.Error
                            );
                            switch (dialogResult)
                            {
                                case DialogResult.Yes:
                                    watchdog.Dispose();
                                    watchdog.Start();
                                    break;
                                case DialogResult.No:
                                default:
                                    Environment.Exit(0);
                                    break;
                            }
                        });
                    };
                    this.Closing += (sender, e) =>
                    {
                        watchdog.Dispose();
                    };
                }
            }
        }

        private MenuBar ConstructMenu()
        {
            var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
            quitCommand.Executed += (sender, e) => Application.Instance.Quit();

            var aboutCommand = new Command { MenuText = "About...", Shortcut = Keys.F1 };
            aboutCommand.Executed += (sender, e) => AboutDialog.ShowDialog(this);

            var resetSettings = new Command { MenuText = "Reset to defaults" };
            resetSettings.Executed += async (sender, e) => await ResetSettingsDialog();

            var loadSettings = new Command { MenuText = "Load settings...", Shortcut = Application.Instance.CommonModifier | Keys.O };
            loadSettings.Executed += async (sender, e) => await LoadSettingsDialog();

            var saveSettingsAs = new Command { MenuText = "Save settings as...", Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.S };
            saveSettingsAs.Executed += async (sender, e) => await SaveSettingsDialog();

            var saveSettings = new Command { MenuText = "Save settings", Shortcut = Application.Instance.CommonModifier | Keys.S };
            saveSettings.Executed += async (sender, e) => await SaveSettings();

            var applySettings = new Command { MenuText = "Apply settings", Shortcut = Application.Instance.CommonModifier | Keys.Enter };
            applySettings.Executed += async (sender, e) => await ApplySettings();

            var refreshPresets = new Command { MenuText = "Refresh presets" };
            refreshPresets.Executed += async (sender, e) => await RefreshPresets();

            var savePreset = new Command { MenuText = "Save as preset..." };
            savePreset.Executed += async (sender, e) => await SavePresetDialog();

            var detectTablet = new Command { MenuText = "Detect tablet", Shortcut = Application.Instance.CommonModifier | Keys.D };
            detectTablet.Executed += async (sender, e) => await DetectTablet();

            var showTabletDebugger = new Command { MenuText = "Tablet debugger..." };
            showTabletDebugger.Executed += (sender, e) => App.Current.DebuggerWindow.Show();

            var deviceStringReader = new Command { MenuText = "Device string reader..." };
            deviceStringReader.Executed += (sender, e) => App.Current.StringReaderWindow.Show();

            var configurationEditor = new Command { MenuText = "Open Configuration Editor...", Shortcut = Application.Instance.CommonModifier | Keys.E };
            configurationEditor.Executed += (sender, e) => App.Current.ConfigEditorWindow.Show();

            var pluginManager = new Command { MenuText = "Open Plugin Manager..." };
            pluginManager.Executed += (sender, e) => App.Current.PluginManagerWindow.Show();

            var wikiUrl = new Command { MenuText = "Open Wiki..." };
            wikiUrl.Executed += (sender, e) => DesktopInterop.Open(WikiUrl);

            var showGuide = new Command { MenuText = "Show guide..." };
            showGuide.Executed += (sender, e) => App.Current.StartupGreeterWindow.Show();

            var exportDiagnostics = new Command { MenuText = "Export diagnostics..." };
            exportDiagnostics.Executed += async (sender, e) => await ExportDiagnostics();

            var updater = new Command { MenuText = "Check for updates..." };
            updater.Executed += (sender, e) => Current.UpdaterWindow.Show();

            var menuBar = new MenuBar
            {
                Items =
                {
                    // File submenu
                    new ButtonMenuItem
                    {
                        Text = "&File",
                        Items =
                        {
                            loadSettings,
                            saveSettings,
                            saveSettingsAs,
                            resetSettings,
                            applySettings,
                            new SeparatorMenuItem(),
                            refreshPresets,
                            savePreset,
                            new ButtonMenuItem
                            {
                                Text = "Presets",
                                Items =
                                {
                                    new ButtonMenuItem
                                    {
                                        Text = "No presets loaded",
                                        Enabled = false
                                    }
                                }
                            }
                        }
                    },
                    // Tablets submenu
                    new ButtonMenuItem
                    {
                        Text = "Tablets",
                        Items =
                        {
                            detectTablet,
                            showTabletDebugger,
                            deviceStringReader,
                            configurationEditor
                        }
                    },
                    // Plugins submenu
                    new ButtonMenuItem
                    {
                        Text = "Plugins",
                        Items =
                        {
                            pluginManager
                        }
                    },
                    new ButtonMenuItem
                    {
                        Text = "&Help",
                        Items =
                        {
                            wikiUrl,
                            exportDiagnostics,
                            showGuide
                        }
                    }
                },
                ApplicationItems =
                {
                    // application (OS X) or file menu (others)
                },
                QuitItem = quitCommand,
                AboutItem = aboutCommand
            };

            switch (SystemInterop.CurrentPlatform)
            {
                case PluginPlatform.Windows:
                case PluginPlatform.MacOS:
                {
                    menuBar.Items.GetSubmenu("&Help").Items.Add(updater);
                    break;
                }
            }

            return menuBar;
        }

        private void SetTitle(IEnumerable<TabletReference> tablets = null)
        {
            string prefix = $"OpenTabletDriver v{App.Version} - ";
            if (tablets?.Any() ?? false)
            {
                // Limit to 3 tablets in the title
                int numTablets = Math.Min(tablets.Count(), 3);
                this.Title = prefix + string.Join(", ", tablets.Take(numTablets).Select(t => t.Properties.Name));
            }
            else
            {
                this.Title = prefix + "No tablets detected.";
            }
        }

        private void HandleDaemonConnected(object sender, EventArgs e) => Application.Instance.AsyncInvoke(async () =>
        {
            // Hook events after the instance is (re)instantiated
            Log.Output += async (sender, message) => await Driver.Instance.WriteMessage(message);
            Driver.TabletsChanged += (sender, tablet) => SetTitle(tablet);

            // Load the application information from the daemon
            AppInfo.Current = await Driver.Instance.GetApplicationInfo();

            // Load any new plugins
            AppInfo.PluginManager.Load();

            // Show the startup greeter
            if (!File.Exists(AppInfo.Current.SettingsFile) && this.WindowState != WindowState.Minimized)
                App.Current.StartupGreeterWindow.Show();

            // Synchronize settings
            await SyncSettings();

            // Set window content
            base.Menu = menu ??= ConstructMenu();
            base.Content = mainPanel ??= new TabletSwitcherPanel
            {
                CommandsControl = new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalContentAlignment = HorizontalAlignment.Right,
                    Spacing = 5,
                    Items =
                    {
                        new Button(async (s, e) => await SaveSettings())
                        {
                            Text = "Save"
                        },
                        new Button(async (s, e) => await ApplySettings())
                        {
                            Text = "Apply"
                        }
                    }
                }
            };

            // Update preset options in File menu and tray icon
            await RefreshPresets();

            // Update title to new instance
            if (await Driver.Instance.GetTablets() is IEnumerable<TabletReference> tablets)
                SetTitle(tablets);
        });

        private void HandleDaemonDisconnected(object sender, EventArgs e) => Application.Instance.AsyncInvoke(async () =>
        {
            // Hide all controls until reconnected
            base.Content = placeholder;
            base.Menu = null;
            // Attempt to reconnect
            await Driver.Connect();
        });

        private async Task ResetSettings()
        {
            await Driver.Instance.ResetSettings();
            App.Current.Settings = await Driver.Instance.GetSettings();
        }

        private async Task ResetSettingsDialog()
        {
            if (MessageBox.Show("Reset settings to default?", "Reset to defaults", MessageBoxButtons.OKCancel, MessageBoxType.Question) == DialogResult.Ok)
                await ResetSettings();
        }

        private async Task SyncSettings()
        {
            App.Current.Settings = await Driver.Instance.GetSettings();
        }

        private async Task LoadSettingsDialog()
        {
            var fileDialog = new OpenFileDialog
            {
                Title = "Load OpenTabletDriver settings...",
                Directory = new Uri(Eto.EtoEnvironment.GetFolderPath(Eto.EtoSpecialFolder.Documents)),
                Filters =
                {
                    new FileFilter("OpenTabletDriver Settings (*.json)", ".json")
                }
            };
            switch (fileDialog.ShowDialog(this))
            {
                case DialogResult.Ok:
                case DialogResult.Yes:
                    var file = new FileInfo(fileDialog.FileName);
                    if (file.Exists)
                    {
                        App.Current.Settings = Settings.Deserialize(file);
                        await Driver.Instance.SetSettings(App.Current.Settings);
                    }
                    break;
            }
        }

        private async Task SaveSettingsDialog()
        {
            var fileDialog = new SaveFileDialog
            {
                Title = "Save OpenTabletDriver settings...",
                Directory = new Uri(Eto.EtoEnvironment.GetFolderPath(Eto.EtoSpecialFolder.Documents)),
                Filters =
                {
                    new FileFilter("OpenTabletDriver Settings (*.json)", ".json")
                }
            };
            switch (fileDialog.ShowDialog(this))
            {
                case DialogResult.Ok:
                case DialogResult.Yes:
                    var file = new FileInfo(fileDialog.FileName);
                    if (App.Current.Settings is Settings settings)
                    {
                        settings.Serialize(file);
                        await ApplySettings();
                    }
                    break;
            }
        }

        private async Task SaveSettings()
        {
            if (App.Current.Settings is Settings settings)
            {
                if (settings.Profiles.Any(p => p.AbsoluteModeSettings.Tablet.Width + p.AbsoluteModeSettings.Tablet.Height == 0))
                {
                    var result = MessageBox.Show(
                        "Warning: Your tablet area is invalid. Saving this configuration may cause problems." + Environment.NewLine +
                        "Are you sure you want to save your configuration?",
                        MessageBoxButtons.YesNo,
                        MessageBoxType.Warning
                    );
                    if (result != DialogResult.Yes)
                        return;
                }

                var appInfo = await Driver.Instance.GetApplicationInfo();
                settings.Serialize(new FileInfo(appInfo.SettingsFile));
                await ApplySettings();
            }
        }

        private async Task ApplySettings()
        {
            try
            {
                if (App.Current.Settings is Settings settings)
                    await Driver.Instance.SetSettings(settings);
            }
            catch (StreamJsonRpc.RemoteInvocationException riex) when (riex.ErrorData is JObject err)
            {
                var type = (string)err["type"];
                var message = (string)err["message"];
                var stack = (string)err["stack"];
                var logMessage = new LogMessage
                {
                    Group = type,
                    Message = message,
                    StackTrace = stack
                };
                Log.Write(logMessage);
            }
        }

        private Task LoadPresets()
        {
            var presetDir = new DirectoryInfo(AppInfo.Current.PresetDirectory);

            if (!presetDir.Exists)
            {
                presetDir.Create();
                Log.Write("Settings", $"The preset directory '{presetDir.FullName}' has been created");
            }

            AppInfo.PresetManager.Refresh();
            return Task.CompletedTask;
        }

        private Task RefreshPresets()
        {
            LoadPresets();

            if (trayIcon != null) // Check non-Linux
                trayIcon.RefreshMenuItems();

            // Update File submenu
            var presets = AppInfo.PresetManager.GetPresets();
            var presetsMenu = menu.Items.GetSubmenu("&File").Items.GetSubmenu("Presets") as ButtonMenuItem;
            presetsMenu.Items.Clear();

            if (presets.Count != 0)
            {
                foreach (var preset in presets)
                {
                    var presetItem = new ButtonMenuItem
                    {
                        Text = preset.Name
                    };
                    presetItem.Click += PresetButtonHandler;

                    presetsMenu.Items.Add(presetItem);
                }
            }
            else
            {
                var emptyPresetsItem = new ButtonMenuItem
                {
                    Text = "No presets loaded",
                    Enabled = false
                };

                presetsMenu.Items.Add(emptyPresetsItem);
            }

            return Task.CompletedTask;
        }

        private async Task SavePresetDialog()
        {
            var fileDialog = new SaveFileDialog
            {
                Title = "Save OpenTabletDriver settings as preset...",
                Directory = new Uri(AppInfo.Current.PresetDirectory),
                Filters =
                {
                    new FileFilter("OpenTabletDriver Settings (*.json)", ".json")
                }
            };
            switch (fileDialog.ShowDialog(this))
            {
                case DialogResult.Ok:
                case DialogResult.Yes:
                    var file = new FileInfo(fileDialog.FileName + (fileDialog.FileName.EndsWith(".json") ? "" : ".json"));
                    if (App.Current.Settings is Settings settings)
                        settings.Serialize(file);
                    await RefreshPresets();
                    break;
            }
        }

        public static void PresetButtonHandler(object sender, EventArgs e)
        {
            var presetName = (sender as ButtonMenuItem).Text;
            var preset = AppInfo.PresetManager.FindPreset(presetName);
            App.Current.Settings = preset.GetSettings();
            App.Driver.Instance.SetSettings(App.Current.Settings);
            Log.Write("Settings", $"Applied preset '{preset.Name}'");
        }

        private async Task DetectTablet()
        {
            await Driver.Instance.DetectTablets();
            await Driver.Instance.SetSettings(await Driver.Instance.GetSettings());
        }

        private async Task ExportDiagnostics()
        {
            try
            {
                var log = await Driver.Instance.GetCurrentLog();
                var diagnosticDump = new DiagnosticInfo(log, await Driver.Instance.GetDevices());
                var fileDialog = new SaveFileDialog
                {
                    Title = "Save diagnostic information to...",
                    Directory = new Uri(Eto.EtoEnvironment.GetFolderPath(Eto.EtoSpecialFolder.Documents)),
                    Filters =
                    {
                        new FileFilter("Diagnostic information", ".json")
                    }
                };
                switch (fileDialog.ShowDialog(this))
                {
                    case DialogResult.Ok:
                    case DialogResult.Yes:
                        var file = new FileInfo(fileDialog.FileName);
                        if (file.Exists)
                            file.Delete();
                        using (var fs = file.OpenWrite())
                        using (var sw = new StreamWriter(fs))
                            await sw.WriteLineAsync(diagnosticDump.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                ex.ShowMessageBox();
            }
        }

        private async Task CheckForUpdates()
        {
            if (await Driver.Instance.HasUpdate())
            {
                var result = MessageBox.Show(
                    "An update to OpenTabletDriver is available. Do you wish to install it?",
                    "Update",
                    MessageBoxButtons.YesNo
                );

                if (result == DialogResult.Yes)
                    Current.UpdaterWindow.Show();
            }
        }
    }
}
