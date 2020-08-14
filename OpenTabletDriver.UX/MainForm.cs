using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eto.Forms;
using Eto.Drawing;
using OpenTabletDriver.UX.Controls;
using OpenTabletDriver.UX.Windows;
using TabletDriverLib;
using TabletDriverPlugin;
using TabletDriverPlugin.Tablet;
using TabletDriverPlugin.Platform.Display;
using TabletDriverLib.Diagnostics;
using NativeLib;
using TabletDriverLib.Plugins;
using TabletDriverPlugin.Output;

namespace OpenTabletDriver.UX
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            Application.Instance.UnhandledException += App.UnhandledException;

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
            relativeConfig = ConstructSensitivityControls();
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

            var bindingLayout = ConstructBindingLayout();

            filterEditor = ConstructPluginManager<IFilter>(
                () => App.Settings.Filters.Contains(filterEditor.SelectedPlugin.Path),
                (sender, enabled) =>
                {
                    var path = filterEditor.SelectedPlugin.Path;
                    if (enabled && !App.Settings.Filters.Contains(path))
                        App.Settings.Filters.Add(path);
                    else if (!enabled && App.Settings.Filters.Contains(path))
                        App.Settings.Filters.Remove(path);
                }
            );

            toolEditor = ConstructPluginManager<ITool>(
                () => App.Settings.Tools.Contains(toolEditor.SelectedPlugin.Path),
                (sender, enabled) =>
                {
                    var path = toolEditor.SelectedPlugin.Path;
                    if (enabled && !App.Settings.Tools.Contains(path))
                        App.Settings.Tools.Add(path);
                    else if (!enabled && App.Settings.Tools.Contains(path))
                        App.Settings.Tools.Remove(path);
                }
            );

            // Main Content
            return new TabControl
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
                        Content = bindingLayout
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
                        Text = "Console",
                        Content = new LogView()
                    }
                }
            };
        }

        private Control ConstructAreaConfig(Control displayControl, Control tabletControl)
        {
            return new StackLayout
            {
                Visible = false,
                Spacing = SystemInfo.CurrentPlatform == RuntimePlatform.Windows ? 0 : 5,
                Items =
                {
                    new StackLayoutItem(displayControl, HorizontalAlignment.Stretch, true),
                    new StackLayoutItem(tabletControl, HorizontalAlignment.Stretch, true)
                }
            };
        }

        private Control ConstructTabletArea()
        {
            tabletAreaEditor = new AreaEditor("mm", true);
            tabletAreaEditor.AreaDisplay.InvalidSizeError = "No tablet detected.";

            this.SettingsChanged += (settings) =>
            {
                tabletAreaEditor.Bind(c => c.ViewModel.Width, settings, m => m.TabletWidth);
                tabletAreaEditor.Bind(c => c.ViewModel.Height, settings, m => m.TabletHeight);
                tabletAreaEditor.Bind(c => c.ViewModel.X, settings, m => m.TabletX);
                tabletAreaEditor.Bind(c => c.ViewModel.Y, settings, m => m.TabletY);
                tabletAreaEditor.Bind(c => c.ViewModel.Rotation, settings, m => m.TabletRotation);
            };

            var lockAr = tabletAreaEditor.AppendCheckBoxMenuItem("Lock aspect ratio", (value) => Settings.LockAspectRatio = value);
            this.SettingsChanged += (settings) =>
            {
                lockAr.Checked = settings.LockAspectRatio;
            };

            var areaClipping = tabletAreaEditor.AppendCheckBoxMenuItem("Area clipping", (value) => Settings.EnableClipping = value);
            this.SettingsChanged += (settings) =>
            {
                areaClipping.Checked = settings.EnableClipping;
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
            this.SettingsChanged += (settings) =>
            {
                displayAreaEditor.Bind(c => c.ViewModel.Width, settings, m => m.DisplayWidth);
                displayAreaEditor.Bind(c => c.ViewModel.Height, settings, m => m.DisplayHeight);
                displayAreaEditor.Bind(c => c.ViewModel.X, settings, m => m.DisplayX);
                displayAreaEditor.Bind(c => c.ViewModel.Y, settings, m => m.DisplayY);
            };
            displayAreaEditor.AppendMenuItemSeparator();
            foreach (var display in TabletDriverLib.Interop.Platform.VirtualScreen.Displays)
                displayAreaEditor.AppendMenuItem($"Set to {display}",
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
                            virtualScreen = TabletDriverLib.Interop.Platform.VirtualScreen;
                            displayAreaEditor.ViewModel.X = display.Position.X + virtualScreen.Position.X + (display.Width / 2);
                            displayAreaEditor.ViewModel.Y = display.Position.Y + virtualScreen.Position.Y + (display.Height / 2);
                        }
                    });

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
            this.SettingsChanged += (settings) =>
            {
                var mode = control.OutputModes.FirstOrDefault(t => t.Path == App.Settings.OutputMode);
                control.SelectedIndex = control.OutputModes.IndexOf(mode);
            };
            return control;
        }

        private Control ConstructSensitivityControls()
        {
            var xSensBox = ConstructSensitivityEditor(
                "X Sensitivity", 
                (s) => App.Settings.XSensitivity = float.TryParse(s, out var val) ? val : 0f,
                () => App.Settings.XSensitivity.ToString(),
                "px/mm"
            );
            var ySensBox = ConstructSensitivityEditor(
                "Y Sensitivity",
                (s) => App.Settings.YSensitivity = float.TryParse(s, out var val) ? val : 0f,
                () => App.Settings.YSensitivity.ToString(),
                "px/mm"
            );
            
            var resetTimeBox = ConstructSensitivityEditor(
                "Reset Time",
                (s) => App.Settings.ResetTime = TimeSpan.TryParse(s, out var val) ? val : TimeSpan.FromMilliseconds(100),
                () => App.Settings.ResetTime.ToString()
            );

            return new StackLayout
            {
                Visible = false,
                Spacing = 5,
                Orientation = Orientation.Horizontal,
                Items =
                {
                    new StackLayoutItem(xSensBox, true),
                    new StackLayoutItem(ySensBox, true),
                    new StackLayoutItem(resetTimeBox, true)
                }
            };
        }

        private Control ConstructSensitivityEditor(string header, Action<string> setValue, Func<string> getValue, string unit = null)
        {
            var textbox = new TextBox();
            this.SettingsChanged += (settings) =>
            {
                textbox.TextBinding.Bind(getValue, setValue);
            };

            var layout = TableLayout.Horizontal(5, new TableCell(textbox, true));

            if (unit != null)
            {
                var unitControl = new Label
                {
                    Text = unit, 
                    VerticalAlignment = VerticalAlignment.Center
                };
                layout.Rows[0].Cells.Add(unitControl);
            }
            
            return new GroupBox
            {
                Text = header,
                Padding = App.GroupBoxPadding,
                Content = layout
            };
        }

        private Control ConstructBindingLayout()
        {
            var layout = new StackLayout()
            {
                Orientation = Orientation.Horizontal,
                Padding = new Padding(5),
                Spacing = 5
            };

            this.SettingsChanged += (settings) =>
            {
                // Clear layout
                layout.Items.Clear();

                // Tip Binding
                var tipBindingLayout = new StackLayout
                {
                    Spacing = 5
                };
                
                var tipBindingControl = new BindingDisplay(Settings.TipButton);
                tipBindingControl.BindingUpdated += (s, binding) => Settings.TipButton = binding.ToString();

                var tipBindingGroup = new GroupBox
                {
                    Text = "Tip Binding",
                    Padding = App.GroupBoxPadding,
                    Content = tipBindingControl
                };
                tipBindingLayout.Items.Add(new StackLayoutItem(tipBindingGroup, HorizontalAlignment.Stretch, true));

                var tipPressureSlider = new Slider
                {
                    MinValue = 0,
                    MaxValue = 100,
                };
                tipPressureSlider.Value = (int)Settings.TipActivationPressure;
                tipPressureSlider.ValueChanged += (sender, e) => Settings.TipActivationPressure = tipPressureSlider.Value;

                var tipPressureGroup = new GroupBox
                {
                    Text = "Tip Activation Pressure",
                    Padding = App.GroupBoxPadding,
                    Content = tipPressureSlider
                };
                tipBindingLayout.Items.Add(new StackLayoutItem(tipPressureGroup, HorizontalAlignment.Stretch, true));
                layout.Items.Add(new StackLayoutItem(tipBindingLayout, true));

                // Pen Bindings
                var penBindingLayout = new StackLayout
                {
                    Spacing = 5
                };
                for (int i = 0; i < Settings.PenButtons.Count; i++)
                {
                    var penBindingControl = new BindingDisplay(Settings.PenButtons[i])
                    {
                        Tag = i
                    };
                    penBindingControl.BindingUpdated += (sender, binding) =>
                    {
                        var index = (int)(sender as BindingDisplay).Tag;
                        Settings.PenButtons[index] = binding.ToString();
                    };
                    var penBindingGroup = new GroupBox
                    {
                        Text = $"Pen Button {i + 1}",
                        Padding = App.GroupBoxPadding,
                        Content = penBindingControl
                    };
                    penBindingLayout.Items.Add(new StackLayoutItem(penBindingGroup, HorizontalAlignment.Stretch, true));
                }
                layout.Items.Add(new StackLayoutItem(penBindingLayout, true));

                // Aux Bindings
                var auxBindingLayout = new StackLayout
                {
                    Spacing = 5
                };
                for (int i = 0; i < Settings.AuxButtons.Count; i++)
                {
                    var auxBindingControl = new BindingDisplay(Settings.AuxButtons[i])
                    {
                        Tag = i
                    };
                    auxBindingControl.BindingUpdated += (sender, binding) =>
                    {
                        int index = (int)(sender as BindingDisplay).Tag;
                        Settings.AuxButtons[index] = binding.ToString();
                    };
                    var auxBindingGroup = new GroupBox
                    {
                        Text = $"Express Key {i + 1}",
                        Padding = App.GroupBoxPadding,
                        Content = auxBindingControl
                    };
                    auxBindingLayout.Items.Add(new StackLayoutItem(auxBindingGroup, HorizontalAlignment.Stretch, true));
                }
                layout.Items.Add(new StackLayoutItem(auxBindingLayout, true));
            };
            return layout;
        }

        private PluginManager<T> ConstructPluginManager<T>(Func<bool> getMethod, EventHandler<bool> setMethod)
        {
            var editor = new PluginManager<T>();
            editor.GetPluginEnabled = getMethod;
            editor.SetPluginEnabled += setMethod;
            return editor;
        }

        private MenuBar ConstructMenu()
        {
            var quitCommand = new Command { MenuText = "Quit", Shortcut = Application.Instance.CommonModifier | Keys.Q };
            quitCommand.Executed += (sender, e) => Application.Instance.Quit();

            var aboutCommand = new Command { MenuText = "About...", Shortcut = Keys.F1 };
            aboutCommand.Executed += (sender, e) => App.AboutDialog.ShowDialog(this);

            var resetSettings = new Command { MenuText = "Reset to defaults" };
            resetSettings.Executed += async (sender, e) => await ResetSettings();

            var loadSettings = new Command { MenuText = "Load settings...", Shortcut = Application.Instance.CommonModifier | Keys.O };
            loadSettings.Executed += async (sender, e) => await LoadSettingsDialog();

            var saveSettingsAs = new Command { MenuText = "Save settings as...", Shortcut = Application.Instance.CommonModifier | Keys.Shift | Keys.S };
            saveSettingsAs.Executed += async (sender, e) => await SaveSettingsDialog();

            var saveSettings = new Command { MenuText = "Save settings", Shortcut = Application.Instance.CommonModifier | Keys.S };
            saveSettings.Executed += async (sender, e) => await SaveSettings();

            var applySettings = new Command { MenuText = "Apply settings", Shortcut = Application.Instance.CommonModifier | Keys.Enter };
            applySettings.Executed += async (sender, e) => await ApplySettings();

            var detectTablet = new Command { MenuText = "Detect tablet", Shortcut = Application.Instance.CommonModifier | Keys.D };
            detectTablet.Executed += async (sender, e) => await DetectAllTablets();

            var showTabletDebugger = new Command { MenuText = "Tablet debugger..." };
            showTabletDebugger.Executed += (sender, e) => ShowTabletDebugger();

            var configurationEditor = new Command { MenuText = "Open Configuration Editor...", Shortcut = Application.Instance.CommonModifier | Keys.E };
            configurationEditor.Executed += (sender, e) => ShowConfigurationEditor();

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
                            configurationEditor
                        }
                    },
                    new ButtonMenuItem
                    {
                        Text = "&Help",
                        Items =
                        {
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
            Size? size = null;
            Padding? padding = null;
            
            switch (SystemInfo.CurrentPlatform)
            {
                case RuntimePlatform.MacOS:
                    padding = new Padding(10);
                    size = new Size(970, 730);
                    goto default;
                default:
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
                    break;
            }

            this.Padding = padding ?? new Padding(0);
            this.ClientSize = size ?? new Size(960, 720);
        }

        private async void InitializeAsync()
        {
            var appInfo = await App.DriverDaemon.InvokeAsync(d => d.GetApplicationInfo());
            var pluginDir = new DirectoryInfo(appInfo.PluginDirectory);
            if (pluginDir.Exists)
            {
                foreach (var file in pluginDir.EnumerateFiles("*.dll", SearchOption.AllDirectories))
                {
                    await App.DriverDaemon.InvokeAsync(d => d.ImportPlugin(file.FullName));
                    PluginManager.AddPlugin(file);
                }
            }

            Content = ConstructMainControls();

            if (await App.DriverDaemon.InvokeAsync(d => d.GetTablet()) is TabletProperties tablet)
            {
                SetTabletAreaDimensions(tablet);
            }
            else
            {
                await DetectAllTablets();
            }

            var settingsFile = new FileInfo(appInfo.SettingsFile);
            if (await App.DriverDaemon.InvokeAsync(d => d.GetSettings()) is Settings settings)
            {
                Settings = settings;
            }
            else if (settingsFile.Exists)
            {
                Settings = Settings.Deserialize(settingsFile);
                await App.DriverDaemon.InvokeAsync(d => d.SetSettings(Settings));
            }
            else
            {
                await ResetSettings();
            }

            var virtualScreen = TabletDriverLib.Interop.Platform.VirtualScreen;
            displayAreaEditor.ViewModel.MaxWidth = virtualScreen.Width;
            displayAreaEditor.ViewModel.MaxHeight = virtualScreen.Height;
        }

        private Control absoluteConfig, relativeConfig, nullConfig;
        private StackLayout outputConfig;
        private AreaEditor displayAreaEditor, tabletAreaEditor;
        private PluginManager<IFilter> filterEditor;
        private PluginManager<ITool> toolEditor;

        public event Action<Settings> SettingsChanged;
        public Settings Settings
        {
            set
            {
                App.Settings = value;
                SettingsChanged?.Invoke(Settings);
            }
            get => App.Settings;
        }

        private async Task ResetSettings()
        {
            var virtualScreen = TabletDriverLib.Interop.Platform.VirtualScreen;
            var tablet = await App.DriverDaemon.InvokeAsync(d => d.GetTablet());
            Settings = TabletDriverLib.Settings.Defaults;
            Settings.DisplayWidth = virtualScreen.Width;
            Settings.DisplayHeight = virtualScreen.Height;
            Settings.DisplayX = virtualScreen.Width / 2;
            Settings.DisplayY = virtualScreen.Height / 2;
            Settings.TabletWidth = tablet?.Width ?? 0;
            Settings.TabletHeight = tablet?.Height ?? 0;
            Settings.TabletX = tablet?.Width / 2 ?? 0;
            Settings.TabletY = tablet?.Height / 2 ?? 0;
            await App.DriverDaemon.InvokeAsync(d => d.SetSettings(Settings));
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
                        await App.DriverDaemon.InvokeAsync(d => d.SetSettings(Settings));
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

        private async Task SaveSettings()
        {
            var appInfo = await App.DriverDaemon.InvokeAsync(d => d.GetApplicationInfo());
            if (Settings is Settings settings)
            {
                settings.Serialize(new FileInfo(appInfo.SettingsFile));
                await ApplySettings();
            }
        }

        private async Task ApplySettings()
        {
            if (Settings is Settings settings)
                await App.DriverDaemon.InvokeAsync(d => d.SetSettings(settings));
        }

        private async Task DetectAllTablets()
        {
            if (await App.DriverDaemon.InvokeAsync(d => d.DetectTablets()) is TabletProperties tablet)
            {
                var settings = await App.DriverDaemon.InvokeAsync(d => d.GetSettings());
                if (settings != null)
                    await App.DriverDaemon.InvokeAsync(d => d.SetInputHook(settings.AutoHook));
                SetTabletAreaDimensions(tablet);
            }
        }

        private void ShowConfigurationEditor()
        {
            var configEditor = new ConfigurationEditor();
            configEditor.Show();
        }

        private async Task ExportDiagnostics()
        {
            var log = await App.DriverDaemon.InvokeAsync(d => d.GetCurrentLog());
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

        private void SetTabletAreaDimensions(TabletProperties tablet)
        {
            tabletAreaEditor.ViewModel.MaxWidth = tablet.Width;
            tabletAreaEditor.ViewModel.MaxHeight = tablet.Height;
        }

        private void UpdateOutputMode(PluginReference pluginRef)
        {
            var outputMode = pluginRef.GetTypeReference<IOutputMode>();
            bool showAbsolute = outputMode.IsSubclassOf(typeof(AbsoluteOutputMode));
            bool showRelative = outputMode.IsSubclassOf(typeof(RelativeOutputMode));
            bool showNull = !(showAbsolute | showRelative);
            switch (SystemInfo.CurrentPlatform)
            {
                case RuntimePlatform.Linux:
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
