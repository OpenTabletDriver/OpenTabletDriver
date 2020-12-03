using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Tablet.Interpolator;
using OpenTabletDriver.Reflection;
using OpenTabletDriver.UX.Controls;
using OpenTabletDriver.UX.Windows;

namespace OpenTabletDriver.UX
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            Title = "OpenTabletDriver";
            Icon = App.Logo.WithSize(App.Logo.Size);

            Content = ConstructPlaceholderControl();
            Menu = ConstructMenu();

            ApplyPlatformQuirks();

            InitializeAsync();
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
                        Control = new Bitmap(App.Logo.WithSize(256, 256)),
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
            var displayAreaGroup = ConstructDisplayArea();
            var tabletAreaGroup = ConstructTabletArea();

            var outputModeSelector = ConstructOutputModeSelector();
            absoluteConfig = ConstructAreaConfig(displayAreaGroup, tabletAreaGroup);
            relativeConfig = new SensitivityEditor();
            nullConfig = new Panel();

            outputConfig = new StackLayout
            {
                Padding = 5,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(absoluteConfig, HorizontalAlignment.Stretch, true),
                    new StackLayoutItem(relativeConfig, HorizontalAlignment.Stretch, true),
                    new StackLayoutItem(nullConfig, HorizontalAlignment.Stretch, true),
                    new StackLayoutItem(outputModeSelector, HorizontalAlignment.Left, false)
                }
            };

            filterEditor = new PluginSettingStoreCollectionEditor<IFilter>(
                new WeakReference<PluginSettingStoreCollection>(App.Settings?.Filters, true),
                "Filter"
            );

            toolEditor = new PluginSettingStoreCollectionEditor<ITool>(
                new WeakReference<PluginSettingStoreCollection>(App.Settings?.Tools, true),
                "Tool"
            );

            interpolatorEditor = new PluginSettingStoreCollectionEditor<Interpolator>(
                new WeakReference<PluginSettingStoreCollection>(App.Settings?.Interpolators),
                "Interpolator"
            );

            App.SettingsChanged += (settings) => 
            {
                filterEditor.CollectionReference.SetTarget(App.Settings?.Filters);
                toolEditor.CollectionReference.SetTarget(App.Settings?.Tools);
                interpolatorEditor.CollectionReference.SetTarget(App.Settings?.Interpolators);
            };

            // Main Content
            var tabControl = new TabControl
            {
                Pages =
                {
                    new TabPage
                    {
                        Text = "Output",
                        Content = outputConfig
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
                        Content = filterEditor
                    },
                    new TabPage
                    {
                        Text = "Tools",
                        Padding = 5,
                        Content = toolEditor
                    },
                    new TabPage
                    {
                        Text = "Interpolators",
                        Padding = 5,
                        Content = interpolatorEditor
                    },
                    new TabPage
                    {
                        Text = "Console",
                        Padding = 5,
                        Content = new LogView()
                    }
                }
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

        private Control ConstructAreaConfig(Control displayControl, Control tabletControl)
        {
            return new StackLayout
            {
                Visible = false,
                Spacing = SystemInterop.CurrentPlatform == PluginPlatform.Windows ? 0 : 5,
                Items =
                {
                    new StackLayoutItem(displayControl, HorizontalAlignment.Stretch, true),
                    new StackLayoutItem(tabletControl, HorizontalAlignment.Stretch, true)
                }
            };
        }

        private Control ConstructTabletArea()
        {
            tabletAreaEditor = new AreaEditor("mm", enableRotation: true);
            tabletAreaEditor.AreaDisplay.InvalidSizeError = "No tablet detected.";
            tabletAreaEditor.AreaDisplay.ToolTip =
                "You can right click the area editor to enable aspect ratio locking, adjust alignment, or resize the area.";

            void lockToMaxArea(AreaEditor editor, bool lockToMax)
            {
                if (lockToMax)
                    editor.ViewModel.PropertyChanged += editor.LimitArea;
                else
                    editor.ViewModel.PropertyChanged -= editor.LimitArea;
            }

            var lockUsableTabletArea = tabletAreaEditor.AppendCheckBoxMenuItem(
                "Lock to usable area",
                value =>
                {
                    lockToMaxArea(tabletAreaEditor, value);
                    Settings.LockUsableAreaTablet = value;
                }
            );
            
            App.SettingsChanged += (settings) =>
            {
                tabletAreaEditor.Bind(c => c.ViewModel.Width, settings, m => m.TabletWidth);
                tabletAreaEditor.Bind(c => c.ViewModel.Height, settings, m => m.TabletHeight);
                tabletAreaEditor.Bind(c => c.ViewModel.X, settings, m => m.TabletX);
                tabletAreaEditor.Bind(c => c.ViewModel.Y, settings, m => m.TabletY);
                tabletAreaEditor.Bind(c => c.ViewModel.Rotation, settings, m => m.TabletRotation);
                lockUsableTabletArea.Checked = settings.LockUsableAreaTablet;
                lockToMaxArea(tabletAreaEditor, settings.LockUsableAreaTablet);
            };

            var lockAr = tabletAreaEditor.AppendCheckBoxMenuItem("Lock aspect ratio", (value) => Settings.LockAspectRatio = value);
            App.SettingsChanged += (settings) =>
            {
                lockAr.Checked = settings.LockAspectRatio;
            };

            var areaClipping = tabletAreaEditor.AppendCheckBoxMenuItem("Area clipping", (value) => Settings.EnableClipping = value);
            App.SettingsChanged += (settings) =>
            {
                areaClipping.Checked = settings.EnableClipping;
            };

            var ignoreOutsideArea = tabletAreaEditor.AppendCheckBoxMenuItem("Ignore reports outside area", (value) => Settings.EnableAreaLimiting = value);
            App.SettingsChanged += (settings) =>
            {
                ignoreOutsideArea.Checked = settings.EnableAreaLimiting;
            };

            var tabletAreaGroup = new GroupBox
            {
                Text = "Tablet Area",
                Padding = App.GroupBoxPadding,
                Content = tabletAreaEditor
            };
            return tabletAreaGroup;
        }

        private Control ConstructDisplayArea()
        {
            displayAreaEditor = new AreaEditor("px");
            displayAreaEditor.AreaDisplay.ToolTip =
                "You can right click the area editor to set the area to a display, adjust alignment, or resize the area.";

            void lockToMaxArea(AreaEditor editor, bool lockToMax)
            {
                if (lockToMax)
                    editor.ViewModel.PropertyChanged += editor.LimitArea;
                else
                    editor.ViewModel.PropertyChanged -= editor.LimitArea;
            }

            var lockUsableDisplayArea = displayAreaEditor.AppendCheckBoxMenuItem(
                "Lock to usable area",
                value =>
                {
                    lockToMaxArea(displayAreaEditor, value);
                    Settings.LockUsableAreaDisplay = value;
                }
            );
            
            App.SettingsChanged += (settings) =>
            {
                displayAreaEditor.Bind(c => c.ViewModel.Width, settings, m => m.DisplayWidth);
                displayAreaEditor.Bind(c => c.ViewModel.Height, settings, m => m.DisplayHeight);
                displayAreaEditor.Bind(c => c.ViewModel.X, settings, m => m.DisplayX);
                displayAreaEditor.Bind(c => c.ViewModel.Y, settings, m => m.DisplayY);
                lockUsableDisplayArea.Checked = settings.LockUsableAreaDisplay;
                lockToMaxArea(displayAreaEditor, settings.LockUsableAreaDisplay);
            };

            displayAreaEditor.AppendMenuItemSeparator();

            foreach (var display in SystemInterop.VirtualScreen.Displays)
            {
                displayAreaEditor.AppendMenuItem(
                    $"Set to {display}",
                    () =>
                    {
                        displayAreaEditor.ViewModel.Width = display.Width;
                        displayAreaEditor.ViewModel.Height = display.Height;
                        if (display is IVirtualScreen virtualScreen)
                        {
                            displayAreaEditor.ViewModel.X = virtualScreen.Width / 2;
                            displayAreaEditor.ViewModel.Y = virtualScreen.Height / 2;
                        }
                        else
                        {
                            virtualScreen = SystemInterop.VirtualScreen;
                            displayAreaEditor.ViewModel.X = display.Position.X + virtualScreen.Position.X + (display.Width / 2);
                            displayAreaEditor.ViewModel.Y = display.Position.Y + virtualScreen.Position.Y + (display.Height / 2);
                        }
                    }
                );
            }

            var displayAreaGroup = new GroupBox
            {
                Text = "Display Area",
                Padding = App.GroupBoxPadding,
                Content = displayAreaEditor
            };
            return displayAreaGroup;
        }

        private Control ConstructOutputModeSelector()
        {
            var control = new OutputModeSelector
            {
                Width = 300
            };
            control.SelectedModeChanged += (sender, mode) =>
            {
                App.Settings.OutputMode = mode.Path;
                UpdateOutputMode(mode);
            };
            App.SettingsChanged += (settings) =>
            {
                var mode = control.OutputModes.FirstOrDefault(t => t.Path == App.Settings.OutputMode);
                control.SelectedIndex = control.OutputModes.IndexOf(mode);
            };
            return control;
        }

        private MenuBar ConstructMenu()
        {
            var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
            quitCommand.Executed += (sender, e) => Application.Instance.Quit();

            var aboutCommand = new Command { MenuText = "About...", Shortcut = Keys.F1 };
            aboutCommand.Executed += (sender, e) => App.AboutDialog.ShowDialog(this);

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

            var pluginsDirectory = new Command { MenuText = "Open plugins directory..." };
            pluginsDirectory.Executed += (sender, e) => SystemInterop.Open(AppInfo.Current.PluginDirectory);

            var pluginsRepository = new Command { MenuText = "Open plugins repository..." };
            pluginsRepository.Executed += (sender, e) => SystemInterop.Open(App.PluginRepositoryUrl);

            var faqUrl = new Command { MenuText = "Open FAQ Page..." };
            faqUrl.Executed += (sender, e) => SystemInterop.Open(App.FaqUrl);

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
                            pluginsDirectory,
                            pluginsRepository
                        }
                    },
                    new ButtonMenuItem
                    {
                        Text = "&Help",
                        Items =
                        {
                            faqUrl,
                            exportDiagnostics
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

        private void ApplyPlatformQuirks()
        {
            this.Padding = SystemInterop.CurrentPlatform switch
            {
                PluginPlatform.MacOS => new Padding(10),
                _                     => new Padding(0)
            };

            this.ClientSize = SystemInterop.CurrentPlatform switch
            {
                PluginPlatform.MacOS => new Size(970, 770),
                _ => new Size(960, 760)
            };

            bool enableTrayIcon = SystemInterop.CurrentPlatform switch
            {
                PluginPlatform.Windows => true,
                PluginPlatform.MacOS   => true,
                _                       => false
            };

            bool enableDaemonWatchdog = SystemInterop.CurrentPlatform switch
            {
                PluginPlatform.Windows => true,
                PluginPlatform.MacOS   => true,
                _                       => false,
            };

            if (enableTrayIcon)
            {
                var trayIcon = new TrayIcon(this);
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
                            this.Visible = false;
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
                        var dialogResult = MessageBox.Show(
                            this,
                            "Fatal: The OpenTabletDriver Daemon has exited. Do you want to restart OpenTabletDriver?",
                            "OpenTabletDriver Fatal Error",
                            MessageBoxButtons.YesNo
                        );
                        switch (dialogResult)
                        {
                            case DialogResult.Yes:
                                Application.Instance.Restart();
                                break;
                            case DialogResult.No:
                            default:
                                Application.Instance.Quit();
                                break;
                        }
                    };
                    this.Closing += (sender, e) =>
                    {
                        watchdog.Dispose();
                    };
                }
            }
        }

        private async void InitializeAsync()
        {
            try
            {
                await App.Driver.Connect();
            }
            catch (TimeoutException)
            {
                MessageBox.Show("Daemon connection timed out after some time. Verify that the daemon is running.", "Daemon Connection Timed Out");
                Application.Instance.Quit();
            }

            AppInfo.Current = await App.Driver.Instance.GetApplicationInfo();

            AppInfo.PluginManager.LoadPlugins(new DirectoryInfo(AppInfo.Current.PluginDirectory));
            Log.Output += async (sender, message) => await App.Driver.Instance.WriteMessage(message);

            Content = ConstructMainControls();

            if (await App.Driver.Instance.GetTablet() is TabletState tablet)
            {
                SetTabletAreaDimensions(tablet);
            }
            App.Driver.Instance.TabletChanged += (sender, tablet) => SetTabletAreaDimensions(tablet);

            var settingsFile = new FileInfo(AppInfo.Current.SettingsFile);
            if (await App.Driver.Instance.GetSettings() is Settings settings)
            {
                Settings = settings;
            }
            else if (settingsFile.Exists)
            {
                try
                {
                    Settings = Serialization.Deserialize<Settings>(settingsFile);
                    await App.Driver.Instance.SetSettings(Settings);
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

            displayAreaEditor.ViewModel.Background = from disp in SystemInterop.VirtualScreen.Displays
                where !(disp is IVirtualScreen)
                select new RectangleF(disp.Position.X, disp.Position.Y, disp.Width, disp.Height);
        }

        private Control absoluteConfig, relativeConfig, nullConfig;
        private StackLayout outputConfig;
        private AreaEditor displayAreaEditor, tabletAreaEditor;
        private PluginSettingStoreCollectionEditor<IFilter> filterEditor;
        private PluginSettingStoreCollectionEditor<ITool> toolEditor;
        private PluginSettingStoreCollectionEditor<Interpolator> interpolatorEditor;

        public Settings Settings
        {
            set => App.Settings = value;
            get => App.Settings;
        }

        private async Task ResetSettings(bool force = true)
        {
            if (!force && MessageBox.Show("Reset settings to default?", "Reset to defaults", MessageBoxButtons.OKCancel, MessageBoxType.Question) != DialogResult.Ok)
                return;

            await App.Driver.Instance.ResetSettings();
            Settings = await App.Driver.Instance.GetSettings();
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
                        Settings = Serialization.Deserialize<Settings>(file);
                        await App.Driver.Instance.SetSettings(Settings);
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
                        Serialization.Serialize(file, settings);
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

                var appInfo = await App.Driver.Instance.GetApplicationInfo();
                Serialization.Serialize(new FileInfo(appInfo.SettingsFile), settings);
                await ApplySettings();
            }
        }

        private async Task ApplySettings()
        {
            try
            {
                if (Settings is Settings settings)
                    await App.Driver.Instance.SetSettings(settings);
            }
            catch (StreamJsonRpc.RemoteInvocationException riex)
            {
                Log.Exception(riex.InnerException);
            }
        }

        private async Task DetectAllTablets()
        {
            if (await App.Driver.Instance.DetectTablets() is TabletState tablet)
            {
                var settings = await App.Driver.Instance.GetSettings();
                if (settings != null)
                    await App.Driver.Instance.EnableInput(settings.AutoHook);
                SetTabletAreaDimensions(tablet);
            }
        }

        private void ShowConfigurationEditor()
        {
            var configEditor = new ConfigurationEditor();
            configEditor.Show();
        }

        private void ShowDeviceStringReader()
        {
            var stringReader = new DeviceStringReader();
            stringReader.Show();
        }

        private async Task ExportDiagnostics()
        {
            var log = await App.Driver.Instance.GetCurrentLog();
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

        private void SetTabletAreaDimensions(TabletState tablet)
        {
            Application.Instance.AsyncInvoke(() =>
            {
                if (tablet != null)
                {
                    tabletAreaEditor.SetBackground(new RectangleF(0, 0, tablet.Digitizer.Width, tablet.Digitizer.Height));

                    if (Settings != null && Settings.TabletWidth == 0 && Settings.TabletHeight == 0)
                    {
                        Settings.TabletWidth = tablet.Digitizer.Width;
                        Settings.TabletHeight = tablet.Digitizer.Height;
                    }
                }
                else
                {
                    tabletAreaEditor.SetBackground(null);
                }
            });
        }

        private void UpdateOutputMode(PluginReference pluginRef)
        {
            var outputMode = pluginRef.GetTypeReference<IOutputMode>();
            bool showAbsolute = outputMode.IsSubclassOf(typeof(AbsoluteOutputMode));
            bool showRelative = outputMode.IsSubclassOf(typeof(RelativeOutputMode));
            bool showNull = !(showAbsolute | showRelative);
            switch (SystemInterop.CurrentPlatform)
            {
                case PluginPlatform.Linux:
                    absoluteConfig.Visible = showAbsolute;
                    relativeConfig.Visible = showRelative;
                    nullConfig.Visible = showNull;
                    break;
                default:
                    absoluteConfig.Visible = true;
                    relativeConfig.Visible = true;
                    nullConfig.Visible = true;

                    void setVisibilityWorkaround(Control control, bool visibility, int index)
                    {
                        var isContained = outputConfig.Items.Any(d => d.Control == control);
                        if (!isContained & visibility)
                        {
                            if (outputConfig.Items.Count - index - 1 < 0)
                                index = 0;
                            outputConfig.Items.Insert(index, new StackLayoutItem(control, HorizontalAlignment.Stretch, true));
                        }
                        else if (isContained & !visibility)
                        {
                            var item = outputConfig.Items.FirstOrDefault(d => d.Control == control);
                            outputConfig.Items.Remove(item);
                        }
                    }

                    setVisibilityWorkaround(absoluteConfig, showAbsolute, 0);
                    setVisibilityWorkaround(relativeConfig, showRelative, 1);
                    setVisibilityWorkaround(nullConfig, showNull, 2);
                    break;
            }
        }

        private void ShowTabletDebugger()
        {
            var debugger = new TabletDebugger();
            debugger.Show();
        }
    }
}
