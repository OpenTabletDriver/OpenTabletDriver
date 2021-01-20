using System;
using System.IO;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Interpolator;
using OpenTabletDriver.UX.Controls;
using OpenTabletDriver.UX.Windows;
using OpenTabletDriver.UX.Windows.Configurations;
using OpenTabletDriver.UX.Windows.Greeter;

namespace OpenTabletDriver.UX
{
    using static App;

    public partial class MainForm : DesktopForm
    {
        public MainForm()
            : base()
        {
            Title = "OpenTabletDriver";
            ClientSize = new Size(DEFAULT_CLIENT_WIDTH, DEFAULT_CLIENT_HEIGHT);
            Content = ConstructPlaceholderControl();
            Menu = ConstructMenu();

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

        private OutputModeEditor outputModeEditor;
        private BindingEditor bindingEditor;
        private PluginSettingStoreCollectionEditor<IFilter> filterEditor;
        private PluginSettingStoreCollectionEditor<ITool> toolEditor;
        private PluginSettingStoreCollectionEditor<Interpolator> interpolatorEditor;

        public void Refresh()
        {
            bindingEditor = new BindingEditor();
            filterEditor.UpdateStore(Settings?.Filters);
            toolEditor.UpdateStore(Settings?.Tools);
            interpolatorEditor.UpdateStore(Settings?.Interpolators);
            outputModeEditor.Refresh();
        }

        protected override void OnInitializePlatform(EventArgs e)
        {
            base.OnInitializePlatform(e);

            switch (SystemInterop.CurrentPlatform)
            {
                case PluginPlatform.MacOS:
                    this.Padding = 10;
                    break;
            }

            bool enableDaemonWatchdog = SystemInterop.CurrentPlatform switch
            {
                PluginPlatform.Windows => true,
                PluginPlatform.MacOS   => true,
                _                      => false,
            };

            if (SystemInterop.CurrentPlatform == PluginPlatform.MacOS)
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
                    this.ShowInTaskbar = false;
                this.WindowStateChanged += (sender, e) =>
                {
                    switch (this.WindowState)
                    {
                        case WindowState.Normal:
                        case WindowState.Maximized:
                            this.ShowInTaskbar = true;
                            break;
                        case WindowState.Minimized:
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

        private Control ConstructMainControls()
        {
            // Main Content
            var tabControl = new TabControl
            {
                Pages =
                {
                    new TabPage
                    {
                        Text = "Output",
                        Content = outputModeEditor = new OutputModeEditor()
                    },
                    new TabPage
                    {
                        Text = "Bindings",
                        Content = bindingEditor = new BindingEditor()
                    },
                    new TabPage
                    {
                        Text = "Filters",
                        Padding = 5,
                        Content = filterEditor = new PluginSettingStoreCollectionEditor<IFilter>(
                            Settings?.Filters,
                            "Filter"
                        )
                    },
                    new TabPage
                    {
                        Text = "Tools",
                        Padding = 5,
                        Content = toolEditor = new PluginSettingStoreCollectionEditor<ITool>(
                            Settings?.Tools,
                            "Tool"
                        )
                    },
                    new TabPage
                    {
                        Text = "Interpolators",
                        Padding = 5,
                        Content = interpolatorEditor = new PluginSettingStoreCollectionEditor<Interpolator>(
                            Settings?.Interpolators,
                            "Interpolator"
                        )
                    },
                    new TabPage
                    {
                        Text = "Console",
                        Padding = 5,
                        Content = new LogView()
                    }
                }
            };

            SettingsChanged += (settings) =>
            {
                filterEditor.UpdateStore(Settings?.Filters);
                toolEditor.UpdateStore(Settings?.Tools);
                interpolatorEditor.UpdateStore(Settings?.Interpolators);
            };

            var commandsPanel = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                Padding = new Padding(0, 5),
                Spacing = 5,
                Items =
                {
                    new Button(async (s, e) => await SaveSettings(Settings))
                    {
                        Text = "Save"
                    },
                    new Button(async (s, e) => await ApplySettings())
                    {
                        Text = "Apply"
                    }
                }
            };

            return new StackLayout
            {
                Items =
                {
                    new StackLayoutItem(tabControl, HorizontalAlignment.Stretch, true),
                    new StackLayoutItem(commandsPanel, HorizontalAlignment.Right)
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
            resetSettings.Executed += async (sender, e) => await ResetSettings(false);

            var loadSettings = new Command { MenuText = "Load settings...", Shortcut = Application.Instance.CommonModifier | Keys.O };
            loadSettings.Executed += async (sender, e) => await LoadSettingsDialog();

            var saveSettingsAs = new Command { MenuText = "Save settings as...", Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.S };
            saveSettingsAs.Executed += async (sender, e) => await SaveSettingsDialog();

            var saveSettings = new Command { MenuText = "Save settings", Shortcut = Application.Instance.CommonModifier | Keys.S };
            saveSettings.Executed += async (sender, e) => await SaveSettings(Settings);

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
            faqUrl.Executed += (sender, e) => SystemInterop.Open(FaqUrl);

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

            Content = ConstructMainControls();

            if (await Driver.Instance.GetTablet() is TabletState tablet)
                outputModeEditor.SetTabletSize(tablet);

            Driver.Instance.TabletChanged += (sender, tablet) => outputModeEditor.SetTabletSize(tablet);

            await LoadSettings(AppInfo.Current);
        }

        private async Task LoadSettings(AppInfo appInfo = null)
        {
            appInfo ??= await Driver.Instance.GetApplicationInfo();
            var settingsFile = new FileInfo(appInfo.SettingsFile);
            if (await Driver.Instance.GetSettings() is Settings settings)
            {
                Settings = settings;
            }
            else if (settingsFile.Exists)
            {
                try
                {
                    Settings = Settings.Deserialize(settingsFile);
                    await Driver.Instance.SetSettings(Settings);
                }
                catch
                {
                    MessageBox.Show("Failed to load your current settings. They are either out of date or corrupted.", MessageBoxType.Error);
                    await ResetSettings();
                }
            }
            else
            {
                await ResetSettings();
            }

            if (!settingsFile.Exists)
                await ShowFirstStartupGreeter();

            outputModeEditor.SetDisplaySize(SystemInterop.VirtualScreen.Displays);
        }

        private async Task ResetSettings(bool force = true)
        {
            if (!force && MessageBox.Show("Reset settings to default?", "Reset to defaults", MessageBoxButtons.OKCancel, MessageBoxType.Question) != DialogResult.Ok)
                return;

            await Driver.Instance.ResetSettings();
            Settings = await Driver.Instance.GetSettings();
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
                        Settings = Settings.Deserialize(file);
                        await Driver.Instance.SetSettings(Settings);
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
                    if (Settings is Settings settings)
                    {
                        settings.Serialize(file);
                        await ApplySettings();
                    }
                    break;
            }
        }

        private async Task SaveSettings(Settings settings)
        {
            if (settings != null)
            {
                if (settings.TabletWidth == 0 || settings.TabletHeight == 0)
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

        private async Task ApplySettings()
        {
            try
            {
                if (Settings is Settings settings)
                    await Driver.Instance.SetSettings(settings);
            }
            catch (StreamJsonRpc.RemoteInvocationException riex)
            {
                Log.Exception(riex.InnerException);
            }
        }

        private async Task DetectAllTablets()
        {
            if (await Driver.Instance.DetectTablets() is TabletState tablet)
            {
                var settings = await Driver.Instance.GetSettings();
                await Driver.Instance.EnableInput(settings?.AutoHook ?? false);
            }
        }

        private async Task ShowFirstStartupGreeter()
        {
            var greeter = new StartupGreeterWindow(this);
            await greeter.ShowModalAsync();
        }

        private void ShowConfigurationEditor()
        {
            var configEditor = new ConfigurationEditor();
            configEditor.Show();
        }

        private void ShowPluginManager()
        {
            var pluginManager = new PluginManagerWindow();
            pluginManager.Show();
        }

        private void ShowDeviceStringReader()
        {
            var stringReader = new DeviceStringReader();
            stringReader.Show();
        }

        private void ShowTabletDebugger()
        {
            var debugger = new TabletDebugger();
            debugger.Show();
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
