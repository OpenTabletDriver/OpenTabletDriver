using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Diagnostics;
using OpenTabletDriver.Native;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Tablet;
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

            filterEditor = ConstructPluginSettingsEditor<IFilter>(
                "Filter",
                () => App.Settings.Filters.Contains(filterEditor.SelectedPlugin.Path),
                (enabled) =>
                {
                    var path = filterEditor.SelectedPlugin.Path;
                    if (enabled && !App.Settings.Filters.Contains(path))
                        App.Settings.Filters.Add(path);
                    else if (!enabled && App.Settings.Filters.Contains(path))
                        App.Settings.Filters.Remove(path);
                }
            );

            toolEditor = ConstructPluginSettingsEditor<ITool>(
                "Tool",
                () => App.Settings.Tools.Contains(toolEditor.SelectedPlugin.Path),
                (enabled) =>
                {
                    var path = toolEditor.SelectedPlugin.Path;
                    if (enabled && !App.Settings.Tools.Contains(path))
                        App.Settings.Tools.Add(path);
                    else if (!enabled && App.Settings.Tools.Contains(path))
                        App.Settings.Tools.Remove(path);
                }
            );

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
                    new Button(async (s, e) => await SaveSettings())
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
            tabletAreaEditor = new AreaEditor("mm", enableRotation: true);
            tabletAreaEditor.AreaDisplay.InvalidSizeError = "No tablet detected.";
            tabletAreaEditor.AreaDisplay.ToolTip =
                "You can right click the area editor to enable aspect ratio locking, adjust alignment, or resize the area.";

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
            displayAreaEditor.AreaDisplay.ToolTip =
                "You can right click the area editor to set the area to a display, adjust alignment, or resize the area.";

            this.SettingsChanged += (settings) =>
            {
                displayAreaEditor.Bind(c => c.ViewModel.Width, settings, m => m.DisplayWidth);
                displayAreaEditor.Bind(c => c.ViewModel.Height, settings, m => m.DisplayHeight);
                displayAreaEditor.Bind(c => c.ViewModel.X, settings, m => m.DisplayX);
                displayAreaEditor.Bind(c => c.ViewModel.Y, settings, m => m.DisplayY);
            };
            displayAreaEditor.AppendMenuItemSeparator();
            foreach (var display in OpenTabletDriver.Interop.Platform.VirtualScreen.Displays)
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
                            virtualScreen = OpenTabletDriver.Interop.Platform.VirtualScreen;
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
                    MaxValue = 100
                };
                var tipPressureBox = new TextBox();

                tipPressureSlider.ValueChanged += (sender, e) => 
                {
                    settings.TipActivationPressure = tipPressureSlider.Value;
                    tipPressureBox.Text = Settings.TipActivationPressure.ToString();
                    tipPressureBox.CaretIndex = tipPressureBox.Text.Length;
                };

                tipPressureBox.TextChanged += (sender, e) =>
                {
                    Settings.TipActivationPressure = float.TryParse(tipPressureBox.Text, out var val) ? val : 0f;
                    tipPressureSlider.Value = (int)Settings.TipActivationPressure;
                };
                tipPressureBox.Text = Settings.TipActivationPressure.ToString();
                
                var tipPressureLayout = new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    Items = 
                    {
                        new StackLayoutItem(tipPressureSlider, VerticalAlignment.Center, true),
                        new StackLayoutItem(tipPressureBox, VerticalAlignment.Center, false)
                    }
                };

                var tipPressureGroup = new GroupBox
                {
                    Text = "Tip Activation Pressure",
                    Padding = App.GroupBoxPadding,
                    Content = tipPressureLayout
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

        private PluginSettingsEditor<T> ConstructPluginSettingsEditor<T>(string friendlyName, Func<bool> getMethod, Action<bool> setMethod)
        {
            var editor = new PluginSettingsEditor<T>(friendlyName);
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
            resetSettings.Executed += async (sender, e) => await ResetSettings(false);

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

            var pluginsDirectory = new Command { MenuText = "Open plugins directory..." };
            pluginsDirectory.Executed += (sender, e) => SystemInfo.Open(AppInfo.Current.PluginDirectory);

            var pluginsRepository = new Command { MenuText = "Open plugins repository..." };
            pluginsRepository.Executed += (sender, e) => SystemInfo.Open(App.PluginRepositoryUrl);

            var faqUrl = new Command { MenuText = "Open FAQ Page..." };
            faqUrl.Executed += (sender, e) => SystemInfo.Open(App.FaqUrl);

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
            this.Padding = SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.MacOS => new Padding(10),
                _                     => new Padding(0)
            };

            this.ClientSize = SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.MacOS => new Size(970, 770),
                _ => new Size(960, 760)
            };

            bool enableTrayIcon = SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.Windows => true,
                RuntimePlatform.MacOS   => true,
                _                       => false
            };

            bool enableDaemonWatchdog = SystemInfo.CurrentPlatform switch
            {
                RuntimePlatform.Windows => true,
                RuntimePlatform.MacOS   => true,
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
                Log.Output += async (sender, message) => await App.Driver.Instance.WriteMessage(message);
            }
            catch (TimeoutException)
            {
                MessageBox.Show("Daemon connection timed out after some time. Verify that the daemon is running.", "Daemon Connection Timed Out");
                Application.Instance.Quit();
            }

            var appInfo = await App.Driver.Instance.GetApplicationInfo();
            var pluginDir = new DirectoryInfo(appInfo.PluginDirectory);
            if (pluginDir.Exists)
            {
                foreach (var file in pluginDir.EnumerateFiles("*.dll", SearchOption.AllDirectories))
                {
                    await App.Driver.Instance.ImportPlugin(file.FullName);
                    PluginManager.AddPlugin(file);
                }
            }

            Content = ConstructMainControls();

            if (await App.Driver.Instance.GetTablet() is TabletStatus tablet)
            {
                SetTabletAreaDimensions(tablet);
            }
            App.Driver.Instance.TabletChanged += (sender, tablet) => SetTabletAreaDimensions(tablet);

            var settingsFile = new FileInfo(appInfo.SettingsFile);
            if (await App.Driver.Instance.GetSettings() is Settings settings)
            {
                Settings = settings;
            }
            else if (settingsFile.Exists)
            {
                try
                {
                    Settings = Settings.Deserialize(settingsFile);
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

            displayAreaEditor.ViewModel.Background = from disp in OpenTabletDriver.Interop.Platform.VirtualScreen.Displays
                where !(disp is IVirtualScreen)
                select new RectangleF(disp.Position.X, disp.Position.Y, disp.Width, disp.Height);
        }

        private Control absoluteConfig, relativeConfig, nullConfig;
        private StackLayout outputConfig;
        private AreaEditor displayAreaEditor, tabletAreaEditor;
        private PluginSettingsEditor<IFilter> filterEditor;
        private PluginSettingsEditor<ITool> toolEditor;

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

        private async Task ResetSettings(bool force = true)
        {
            if (!force && MessageBox.Show("Reset settings to default?", "Reset to defaults", MessageBoxButtons.OKCancel, MessageBoxType.Question) != DialogResult.Ok)
                return;

            var virtualScreen = OpenTabletDriver.Interop.Platform.VirtualScreen;
            var tablet = await App.Driver.Instance.GetTablet();
            Settings = OpenTabletDriver.Settings.Defaults;
            Settings.DisplayWidth = virtualScreen.Width;
            Settings.DisplayHeight = virtualScreen.Height;
            Settings.DisplayX = virtualScreen.Width / 2;
            Settings.DisplayY = virtualScreen.Height / 2;
            Settings.TabletWidth = tablet?.TabletIdentifier?.Width ?? 0;
            Settings.TabletHeight = tablet?.TabletIdentifier?.Height ?? 0;
            Settings.TabletX = tablet?.TabletIdentifier?.Width / 2 ?? 0;
            Settings.TabletY = tablet?.TabletIdentifier?.Height / 2 ?? 0;
            await App.Driver.Instance.SetSettings(Settings);
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
                        settings.Serialize(file);
                        await ApplySettings();
                    }
                    break;
            }
        }

        private async Task SaveSettings()
        {
            var appInfo = await App.Driver.Instance.GetApplicationInfo();
            if (Settings is Settings settings)
            {
                settings.Serialize(new FileInfo(appInfo.SettingsFile));
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
            if (await App.Driver.Instance.DetectTablets() is TabletStatus tablet)
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

        private void SetTabletAreaDimensions(TabletStatus tablet)
        {
            if (tablet != null)
            {
                tabletAreaEditor.ViewModel.Background = new RectangleF[]
                {
                    new RectangleF(0, 0, tablet.TabletIdentifier.Width, tablet.TabletIdentifier.Height)
                };
            }
            else
            {
                tabletAreaEditor.ViewModel.Background = null;
            }
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
