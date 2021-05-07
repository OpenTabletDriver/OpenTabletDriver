using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls;
using OpenTabletDriver.UX.Controls.Output;
using OpenTabletDriver.UX.Tools;
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
            UpdateTitle();
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
                    await LoadSettings();
                    Content = content;
                });
            };

            InitializeAsync();
        }

        private const string NO_TABLET = "No tablet detected";

        private OutputModeEditor outputModeEditor;
        private PluginSettingStoreCollectionEditor<IPositionedPipelineElement<IDeviceReport>> filterEditor;
        private PluginSettingStoreCollectionEditor<ITool> toolEditor;

        private readonly WindowSingleton<ConfigurationEditor> configEditorWindow = new WindowSingleton<ConfigurationEditor>();
        private readonly WindowSingleton<PluginManagerWindow> pluginManagerWindow = new WindowSingleton<PluginManagerWindow>();
        private readonly WindowSingleton<TabletDebugger> debuggerWindow = new WindowSingleton<TabletDebugger>();

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
                        Content = new BindingEditor()
                    },
                    new TabPage
                    {
                        Text = "Filters",
                        Padding = 5,
                        Content = filterEditor = new PluginSettingStoreCollectionEditor<IPositionedPipelineElement<IDeviceReport>>
                        {
                            FriendlyTypeName = "Filter"
                        }
                    },
                    new TabPage
                    {
                        Text = "Tools",
                        Padding = 5,
                        Content = toolEditor = new PluginSettingStoreCollectionEditor<ITool>
                        {
                            FriendlyTypeName = "Tool"
                        }
                    },
                    new TabPage
                    {
                        Text = "Console",
                        Padding = 5,
                        Content = new LogView()
                    }
                }
            };

            filterEditor.StoreCollectionBinding.Bind(App.Current.ProfileBinding.Child(p => p.Filters));
            toolEditor.StoreCollectionBinding.Bind(App.Current.SettingsBinding.Child(s => s.Tools));

            var commandsPanel = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                Padding = new Padding(0, 5),
                Spacing = 5,
                Items =
                {
                    new Button(async (s, e) =>
                    {
                        await SaveSettings();
                        await SaveProfile(App.Current.ProfileCache.HandlerInFocus, App.Current.ProfileCache.ProfileInFocus);
                    })
                    {
                        Text = "Save"
                    },
                    new Button(async (s, e) =>
                    {
                        await ApplySettings();
                        await ApplyProfile(App.Current.ProfileCache.HandlerInFocus, App.Current.ProfileCache.ProfileInFocus);
                    })
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
            resetSettings.Executed += async (sender, e) => await ResetSettingsDialog();

            var loadSettings = new Command { MenuText = "Load settings...", Shortcut = Application.Instance.CommonModifier | Keys.O };
            loadSettings.Executed += async (sender, e) => await LoadSettingsDialog();

            var saveSettingsAs = new Command { MenuText = "Save settings as...", Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.S };
            saveSettingsAs.Executed += async (sender, e) => await SaveSettingsDialog();

            var saveSettings = new Command { MenuText = "Save settings", Shortcut = Application.Instance.CommonModifier | Keys.S };
            saveSettings.Executed += async (sender, e) =>
            {
                await SaveSettings();
                await SaveProfile(App.Current.ProfileCache.HandlerInFocus, App.Current.ProfileCache.ProfileInFocus);
            };

            var applySettings = new Command { MenuText = "Apply settings", Shortcut = Application.Instance.CommonModifier | Keys.Enter };
            applySettings.Executed += async (sender, e) =>
            {
                await ApplySettings();
                await ApplyProfile(App.Current.ProfileCache.HandlerInFocus, App.Current.ProfileCache.ProfileInFocus);
            };

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

            await LoadSettings();
            App.Current.ProfileCache = new ProfileCache();
            await App.Current.ProfileCache.UpdateCache();

            Log.Output += async (sender, message) => await Driver.Instance.WriteMessage(message);

            Content = ConstructMainControls();

            if (await Driver.Instance.GetTablet(App.Current.ProfileCache.HandlerInFocus) is TabletState tablet)
            {
                outputModeEditor.SetTabletSize(tablet);
                UpdateTitle();
            }

            var settingsFile = new FileInfo(AppInfo.Current.SettingsFile);
            if (!settingsFile.Exists && this.WindowState != WindowState.Minimized)
                await ShowFirstStartupGreeter();

            App.Current.ProfileCache.HandlerInFocusChanged += async (_, _) =>
            {
                var id = App.Current.ProfileCache.HandlerInFocus;
                var tablet = await Driver.Instance.GetTablet(id);
                outputModeEditor.SetTabletSize(tablet);
            };

            Driver.Instance.TabletHandlerCreated += (_, _) => Application.Instance.AsyncInvoke(() => UpdateTitle());
            Driver.Instance.TabletHandlerDestroyed += (_, _) => Application.Instance.AsyncInvoke(() => UpdateTitle());
        }

        public async void UpdateTitle()
        {
            var tabletNames = new List<string>();
            if (Driver?.Instance != null)
            {
                foreach (var tabletID in await Driver.Instance.GetActiveTabletHandlerIDs())
                {
                    var tablet = await Driver.Instance.GetTablet(tabletID);
                    tabletNames.Add(tablet.Properties.Name);
                }
            }

            var connectedTablets = tabletNames.Any() ? string.Join(", ", tabletNames) : null;
            await Application.Instance.InvokeAsync(() => this.Title = $"OpenTabletDriver v{App.Version} - {connectedTablets ?? NO_TABLET}");
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

        private async Task LoadSettings()
        {
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
                        App.Current.Settings = Serialization.Deserialize<Settings>(file);
                        await Driver.Instance.SetSettings(App.Current.Settings);
                        await App.Current.ProfileCache.UpdateCache();
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
                        Serialization.Serialize(file, settings);
                        await ApplySettings();
                    }
                    break;
            }
        }

        private async Task SaveSettings()
        {
            if (App.Current.Settings is Settings settings)
            {
                Serialization.Serialize(new FileInfo(AppInfo.Current.SettingsFile), settings);
                await ApplySettings();
            }
        }

        private async Task SaveProfile(TabletHandlerID ID, Profile profile)
        {
            if (profile != null)
            {
                if (profile.TabletWidth == 0 || profile.TabletHeight == 0)
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

                var tabletName = (await Driver.Instance.GetTablet(ID)).Properties.Name;
                Log.Write("Settings", $"Saving profile '{profile.ProfileName}' of '{ID.Value}: {tabletName}'");

                profile.Serialize();
                await ApplyProfile(ID, profile);
            }
        }

        private async Task ApplySettings()
        {
            try
            {
                if (App.Current.Settings is Settings settings)
                    await Driver.Instance.SetSettings(settings);
            }
            catch (StreamJsonRpc.RemoteInvocationException riex)
            {
                Log.Exception(riex.InnerException);
            }
        }

        private async Task ApplyProfile(TabletHandlerID ID, Profile profile)
        {
            try
            {
                if (profile != null)
                    await Driver.Instance.SetProfile(ID, profile);
            }
            catch (StreamJsonRpc.RemoteInvocationException riex)
            {
                Log.Exception(riex.InnerException);
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
