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
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Logging;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls;
using OpenTabletDriver.UX.Windows;
using OpenTabletDriver.UX.Windows.Configurations;
using OpenTabletDriver.UX.Windows.Greeter;
using OpenTabletDriver.UX.Windows.Tablet;

namespace OpenTabletDriver.UX
{
    using static App;

    public class MainForm : DesktopForm
    {
        public MainForm()
            : base()
        {
            this.DataContext = App.Current;

            UpdateTitle(null);
            ClientSize = new Size(DEFAULT_CLIENT_WIDTH, DEFAULT_CLIENT_HEIGHT);
            Content = ConstructPlaceholderControl();

            Driver.Connected += (_, _) =>
            {
                Application.Instance.AsyncInvoke(() => Menu = ConstructMenu());
            };

            Driver.Disconnected += (_, _) =>
            {
                Application.Instance.AsyncInvoke(async () =>
                {
                    var content = this.Content;
                    Content = ConstructPlaceholderControl();
                    await Driver.Connect();
                    await LoadSettings(AppInfo.Current);
                    Content = content;
                });
            };

            InitializeAsync();
        }

        private FileInfo settingsFile;
        private TabletSwitcherPanel tabletSwitcherPanel;

        private WindowSingleton<ConfigurationEditor> configEditorWindow = new WindowSingleton<ConfigurationEditor>();
        private WindowSingleton<PluginManagerWindow> pluginManagerWindow = new WindowSingleton<PluginManagerWindow>();
        private WindowSingleton<TabletDebugger> debuggerWindow = new WindowSingleton<TabletDebugger>();

        protected override void OnInitializePlatform(EventArgs e)
        {
            base.OnInitializePlatform(e);

            switch (DesktopInterop.CurrentPlatform)
            {
                case PluginPlatform.MacOS:
                    this.Padding = 10;
                    break;
            }

            bool enableDaemonWatchdog = DesktopInterop.CurrentPlatform switch
            {
                PluginPlatform.Windows => true,
                PluginPlatform.MacOS   => true,
                _                      => false,
            };

            if (DesktopInterop.CurrentPlatform == PluginPlatform.MacOS)
            {
                var bounds = Screen.PrimaryScreen.Bounds;
                var minWidth = Math.Min(970, bounds.Width * 0.9);
                var minHeight = Math.Min(770, bounds.Height * 0.9);
                this.ClientSize = new Size((int)minWidth, (int)minHeight);
            }

            if (App.EnableTrayIcon)
            {
                var trayIcon = new TrayIcon(this);
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

            if (enableDaemonWatchdog)
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
                                "Fatal: The OpenTabletDriver Daemon has exited. Do you want to restart it and reload OpenTabletDriver?",
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

        private Control ConstructPlaceholderControl()
        {
            return new StackLayout
            {
                Items =
                {
                    new StackLayoutItem(null, true),
                    new StackLayoutItem
                    {
                        Control = new Bitmap(Logo.WithSize(256, 256)),
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    new StackLayoutItem
                    {
                        Control = "Connecting to OpenTabletDriver Daemon...",
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    new StackLayoutItem(null, true)
                }
            };
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
            saveSettings.Executed += async (sender, e) => await SaveSettings(App.Current.Settings);

            var applySettings = new Command { MenuText = "Apply settings", Shortcut = Application.Instance.CommonModifier | Keys.Enter };
            applySettings.Executed += async (sender, e) => await ApplySettings();

            var detectTablet = new Command { MenuText = "Detect tablet", Shortcut = Application.Instance.CommonModifier | Keys.D };
            detectTablet.Executed += async (sender, e) => await DetectAllTablets();

            var showTabletDebugger = new Command { MenuText = "Tablet debugger..." };
            showTabletDebugger.Executed += (sender, e) => ShowTabletDebugger();

            var deviceStringReader = new Command { MenuText = "Device string reader..." };
            deviceStringReader.Executed += (sender, e) => ShowDeviceStringReader();

            var configurationEditor = new Command { MenuText = "Open Configuration Editor...", Shortcut = Application.Instance.CommonModifier | Keys.E };
            configurationEditor.Executed += (sender, e) => ShowConfigurationEditor();

            var pluginManager = new Command { MenuText = "Open Plugin Manager..." };
            pluginManager.Executed += (sender, e) => ShowPluginManager();

            var faqUrl = new Command { MenuText = "Open FAQ Page..." };
            faqUrl.Executed += (sender, e) => DesktopInterop.Open(FaqUrl);

            var showGuide = new Command { MenuText = "Show guide..." };
            showGuide.Executed += async (sender, e) => await ShowFirstStartupGreeter();

            var exportDiagnostics = new Command { MenuText = "Export diagnostics..." };
            exportDiagnostics.Executed += async (sender, e) => await ExportDiagnostics();

            return new MenuBar
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
                            applySettings
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
                            faqUrl,
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
        }

        private async void InitializeAsync()
        {
            try
            {
                await Driver.Connect();
            }
            catch (TimeoutException)
            {
                MessageBox.Show("Daemon connection timed out after some time. Verify that the daemon is running.", "Daemon Connection Timed Out");
                Application.Instance.Quit();
            }

            AppInfo.Current = await Driver.Instance.GetApplicationInfo();
            AppInfo.PluginManager.Load();

            Log.Output += async (sender, message) => await Driver.Instance.WriteMessage(message);

            Content = tabletSwitcherPanel = new TabletSwitcherPanel
            {
                CommandsControl = new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalContentAlignment = HorizontalAlignment.Right,
                    Spacing = 5,
                    Items =
                    {
                        new Button(async (s, e) => await SaveSettings(App.Current.Settings))
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
            
            await LoadSettings(AppInfo.Current);

            if (await Driver.Instance.GetTablets() is IEnumerable<TabletReference> tablets)
            {
                UpdateTitle(tablets);
            }

            if (!settingsFile.Exists && this.WindowState != WindowState.Minimized)
                await ShowFirstStartupGreeter();

            Driver.Instance.TabletsChanged += (sender, tablet) => Application.Instance.AsyncInvoke(() =>
            {
                UpdateTitle(tablet);
            });
        }

        public void UpdateTitle(IEnumerable<TabletReference> tablets)
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

        private async Task LoadSettings(AppInfo appInfo = null)
        {
            appInfo ??= await Driver.Instance.GetApplicationInfo();
            settingsFile = new FileInfo(appInfo.SettingsFile);
            App.Current.Settings = await Driver.Instance.GetSettings();
        }

        private async Task LoadSettingsDialog()
        {
            var fileDialog = new OpenFileDialog
            {
                Title = "Load OpenTabletDriver settings...",
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

        internal async Task SaveSettings(Settings settings)
        {
            if (settings != null)
            {
                if (settings.Profiles.Any(p => p.AbsoluteModeSettings.Tablet.Width + p.AbsoluteModeSettings.Tablet.Height == 0))
                {
                    var result = MessageBox.Show(
                        "Warning: Your tablet area is invalid. Saving this configuration may cause problems." + Environment.NewLine +
                        "Are you sure you want to save your configuration?",
                        MessageBoxButtons.YesNo,
                        MessageBoxType.Warning
                    );
                    switch (result)
                    {
                        case DialogResult.Yes:
                            break;
                        default:
                            return;
                    }
                }

                var appInfo = await Driver.Instance.GetApplicationInfo();
                settings.Serialize(new FileInfo(appInfo.SettingsFile));
                await ApplySettings();
            }
        }

        internal async Task ApplySettings()
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
                Log.OnOutput(logMessage);
            }
        }

        private async Task DetectAllTablets()
        {
            await Driver.Instance.DetectTablets();
        }

        private async Task ShowFirstStartupGreeter()
        {
            var greeter = new StartupGreeterWindow(this);
            await greeter.ShowModalAsync();
        }

        private void ShowConfigurationEditor()
        {
            configEditorWindow.Show();
        }

        public void ShowPluginManager()
        {
            pluginManagerWindow.Show();
        }

        private void ShowDeviceStringReader()
        {
            var stringReader = new DeviceStringReader();
            stringReader.Show();
        }

        private void ShowTabletDebugger()
        {
            debuggerWindow.Show();
        }

        private async Task ExportDiagnostics()
        {
            var log = await Driver.Instance.GetCurrentLog();
            var diagnosticDump = new DiagnosticInfo(log);
            var fileDialog = new SaveFileDialog
            {
                Title = "Exporting diagnostic information...",
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
    }
}
