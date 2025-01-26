using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using Newtonsoft.Json.Linq;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Reflection;
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
            Menu = ConstructLimitedMenu();

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
                    var timeout = Task.Delay(TimeSpan.FromSeconds(15));
                    var result = await Task.WhenAny(Driver.Connect(), timeout);
                    if (result == timeout)
                    {
                        var message = SystemInterop.CurrentPlatform switch
                        {
                            PluginPlatform.Windows => "Connecting to daemon has timed out.\nVerify that OpenTabletDriver.Daemon is running or is in the same folder as OpenTabletDriver.UX",
                            _ => "Connecting to daemon has timed out. Verify that OpenTabletDriver.Daemon is running."
                        };
                        MessageBox.Show(this, message, "Daemon Connection Error", MessageBoxType.Error);
                        Environment.Exit(1);
                    }

                    CheckForUpdates();
                }
                catch (Exception ex)
                {
                    ex.ShowMessageBox();
                    Environment.Exit(2);
                }
            });
        }

        private const int DEFAULT_CLIENT_WIDTH = 960;
        private const int DEFAULT_CLIENT_HEIGHT = 760;

        private MenuBar menu;
        private Placeholder placeholder;
        private TrayIcon trayIcon;

        public bool SilenceDaemonShutdown { get; set; }

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
                case PluginPlatform.Windows:
                    var programPath = AppInfo.ProgramDirectory;
                    var tempPath = Regex.Escape(Path.GetTempPath());
                    var regex = new Regex(@$"^{tempPath}Temp\d+.*?\.zip");

                    if (regex.IsMatch(programPath))
                    {
                        MessageBox.Show(this, $"You are running OpenTabletDriver.UX from a zip file.\n\nPlease extract the zip file to a folder then run OpenTabletDriver.UX from there.", "Error", MessageBoxType.Error);
                        Environment.Exit(1);
                    }
                    break;
                case PluginPlatform.MacOS:
                    this.Padding = 10;
                    break;
            }

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
                    App.DaemonWatchdog = watchdog;

                    AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                    {
                        App.DaemonWatchdog?.Dispose();
                        App.DaemonWatchdog = null;
                    };

                    this.Closing += (sender, e) =>
                    {
                        App.DaemonWatchdog?.Dispose();
                        App.DaemonWatchdog = null;
                    };
                }
            }
        }

        private MenuBar ConstructLimitedMenu()
        {
            var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
            quitCommand.Executed += (sender, e) => Application.Instance.Quit();

            var aboutCommand = new Command { MenuText = "About...", Shortcut = Keys.F1 };
            aboutCommand.Executed += (sender, e) => AboutDialog.ShowDialog(this);

            var wikiUrl = new Command { MenuText = "Open Wiki..." };
            wikiUrl.Executed += (sender, e) => DesktopInterop.Open(WikiUrl);

            var menuBar = new MenuBar
            {
                Items =
                {
                    new ButtonMenuItem
                    {
                        Text = "&Help",
                        Items =
                        {
                            wikiUrl,
                        }
                    }
                },
                QuitItem = quitCommand,
                AboutItem = aboutCommand
            };

            return menuBar;
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
            string prefix = $"OpenTabletDriver v{App.Version}";
            string affix = string.Empty;

            if (tablets?.Any() ?? false)
            {
                // Limit to 3 tablets in the title
                int numTablets = Math.Min(tablets.Count(), 3);
                affix = string.Join(", ", tablets.Take(numTablets).Select(t => t.Properties.Name));
            }

            this.Title = !string.IsNullOrEmpty(affix)
                ? $"{prefix} - {affix}"
                : prefix;
        }

        private void HandleDaemonConnected(object sender, EventArgs e) => Application.Instance.AsyncInvoke(async () =>
        {
            // Hook events after the instance is (re)instantiated
            Log.Output += async (sender, message) => { if (Driver.IsConnected) await Driver.Instance?.WriteMessage(message); };
            Driver.TabletsChanged += (sender, tablet) => SetTitle(tablet);

            // Load full menu
            this.Menu = ConstructMenu();

            // Load the application information from the daemon
            AppInfo.Current = await Driver.Instance.GetApplicationInfo();

            AppInfo.PluginManager = new DesktopPluginManager();
            AppInfo.PresetManager = new PresetManager();

            // Load any new plugins
            AppInfo.PluginManager.Load();

            // Show the startup greeter
            if (!File.Exists(AppInfo.Current.SettingsFile) && this.WindowState != WindowState.Minimized)
                App.Current.StartupGreeterWindow.Show();

            // Synchronize settings
            await SyncSettings();
            Driver.Resynchronize += async (sender, e) => await SyncSettings();

            // Set window content
            base.Menu = menu ??= ConstructMenu();
            base.Content = new TabletSwitcherPanel
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

        private void HandleDaemonDisconnected(object sender, EventArgs e)
        {
            if (SilenceDaemonShutdown)
                return;

            // Hide all controls until reconnected
            Application.Instance.Invoke(() =>
            {
                base.Content = placeholder;
                base.Menu = null;

                MessageBox.Show(
                    "Lost connection to daemon, exiting...",
                    MessageBoxType.Error
                );

                Environment.Exit(1);
            });
        }

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
            var fileDialog = Extensions.OpenFileDialog(
                "Load OpenTabletDriver settings...",
                Eto.EtoEnvironment.GetFolderPath(Eto.EtoSpecialFolder.Documents),
                [new FileFilter("OpenTabletDriver Settings (*.json)", ".json")]
            );

            switch (fileDialog.ShowDialog(this))
            {
                case DialogResult.Ok:
                case DialogResult.Yes:
                    var file = new FileInfo(fileDialog.FileName);
                    if (file.Exists)
                    {
                        if (Settings.TryDeserialize(file, out var settings))
                        {
                            App.Current.Settings = settings;
                            await Driver.Instance.SetSettings(settings);
                        }
                        else
                        {
                            MessageBox.Show(
                                "Invalid settings file.",
                                MessageBoxType.Error
                            );
                        }
                    }
                    break;
            }
        }

        private async Task SaveSettingsDialog()
        {
            var fileDialog = Extensions.SaveFileDialog(
                "Save OpenTabletDriver settings...",
                Eto.EtoEnvironment.GetFolderPath(Eto.EtoSpecialFolder.Documents),
                [new FileFilter("OpenTabletDriver Settings (*.json)", ".json")]
            );

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
            var fileDialog = Extensions.SaveFileDialog(
                "Save OpenTabletDriver settings as preset...",
                AppInfo.Current.PresetDirectory,
                [new FileFilter("OpenTabletDriver Settings (*.json)", ".json")]
            );

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
                var fileDialog = Extensions.SaveFileDialog(
                    "Save diagnostic information to...",
                    Eto.EtoEnvironment.GetFolderPath(Eto.EtoSpecialFolder.Documents),
                    [new FileFilter("Diagnostic information", ".json")]
                );

                switch (fileDialog.ShowDialog(this))
                {
                    case DialogResult.Ok:
                    case DialogResult.Yes:
                        string[] options = { ".json", ".txt", ".log" };
                        var file = new FileInfo(fileDialog.FileName + (options.Any(fileDialog.FileName.EndsWith) ? "" : ".json"));
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

        private void CheckForUpdates()
        {
            Application.Instance.AsyncInvoke(async () =>
            {
                if (await Current.UpdaterWindow.GetWindow().HasUpdates())
                {
                    Current.UpdaterWindow.Show();
                }
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Driver.Disconnected -= HandleDaemonDisconnected;
            base.OnClosing(e);
        }
    }
}
