using Eto.Forms;
using Eto.Drawing;
using OpenTabletDriverUX.Controls;
using System.IO;
using TabletDriverPlugin.Tablet;
using System.Threading.Tasks;
using TabletDriverLib;
using TabletDriverPlugin;
using OpenTabletDriverUX.Windows;
using TabletDriverPlugin.Resident;

namespace OpenTabletDriverUX
{
    public partial class MainForm : Form, IViewModelRoot<MainFormViewModel>
    {
        public MainForm()
        {
            this.DataContext = new MainFormViewModel();
            
            Title = "OpenTabletDriver";
            ClientSize = new Size(960, 750);
            MinimumSize = new Size(960, 750);
            Icon = App.Logo.WithSize(App.Logo.Size);

            Content = ConstructMainControls();
            Menu = ConstructMenu();

            InitializeAsync();
        }
        
        private Control ConstructMainControls()
        {
            displayAreaEditor = new AreaEditor("px");
            ViewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.Settings))
                {
                    displayAreaEditor.Bind(c => c.ViewModel.Width, ViewModel.Settings, m => m.DisplayWidth);
                    displayAreaEditor.Bind(c => c.ViewModel.Height, ViewModel.Settings, m => m.DisplayHeight);
                    displayAreaEditor.Bind(c => c.ViewModel.X, ViewModel.Settings, m => m.DisplayX);
                    displayAreaEditor.Bind(c => c.ViewModel.Y, ViewModel.Settings, m => m.DisplayY);
                }
            };
            
            var displayAreaGroup = new GroupBox
            {
                Text = "Display Area",
                Padding = App.GroupBoxPadding,
                Content = displayAreaEditor
            };

            tabletAreaEditor = new AreaEditor("mm", true);
            ViewModel.PropertyChanged += (sender, e) => 
            {
                if (e.PropertyName == nameof(ViewModel.Settings))
                {
                    tabletAreaEditor.Bind(c => c.ViewModel.Width, ViewModel.Settings, m => m.TabletWidth);
                    tabletAreaEditor.Bind(c => c.ViewModel.Height, ViewModel.Settings, m => m.TabletHeight);
                    tabletAreaEditor.Bind(c => c.ViewModel.X, ViewModel.Settings, m => m.TabletX);
                    tabletAreaEditor.Bind(c => c.ViewModel.Y, ViewModel.Settings, m => m.TabletY);
                    tabletAreaEditor.Bind(c => c.ViewModel.Rotation, ViewModel.Settings, m => m.TabletRotation);
                }
            };

            var tabletAreaGroup = new GroupBox
            {
                Text = "Tablet Area",
                Padding = App.GroupBoxPadding,
                Content = tabletAreaEditor
            };

            var lockAr = new CheckBox
            {
                Text = "Lock Aspect Ratio"
            };

            var areaClipping = new CheckBox
            {
                Text = "Area Clipping"
            };

            ViewModel.PropertyChanged += (sender, e) => 
            {
                if (e.PropertyName == nameof(ViewModel.Settings))
                {
                    lockAr.CheckedBinding.Convert(
                        (b) => b.HasValue ? b.Value : false,
                        (b) => (bool?)b)
                        .BindDataContext(Binding.Property((MainFormViewModel m) => m.Settings.LockAspectRatio));
                    areaClipping.CheckedBinding.Convert(
                        (b) => b.HasValue ? b.Value : false,
                        (b) => (bool?)b)
                        .BindDataContext(Binding.Property((MainFormViewModel m) => m.Settings.EnableClipping));
                }
            };
            
            var areaConfig = new TableLayout
            {
                Padding = new Padding(5),
                Spacing = new Size(5, 5),
                Rows = 
                {
                    new TableRow(new TableCell(displayAreaGroup, true))
                    {
                        ScaleHeight = true
                    },
                    new TableRow(new TableCell(tabletAreaGroup, true))
                    {
                        ScaleHeight = true
                    },
                    new TableRow()
                    {
                        ScaleHeight = false,
                        Cells = 
                        {
                            new StackLayout
                            {
                                Orientation = Orientation.Horizontal,
                                Items =
                                {
                                    lockAr,
                                    areaClipping
                                }
                            }
                        }
                    }
                }
            };

            var xSens = new TextBox();
            ViewModel.PropertyChanged += (sender, e) => 
            {
                if (e.PropertyName == nameof(ViewModel.Settings))
                {
                    xSens.TextBinding.Convert(
                        s => float.TryParse(s, out var v) ? v : 0,
                        f => f.ToString())
                        .BindDataContext(Binding.Property((MainFormViewModel m) => m.Settings.XSensitivity));
                }
            };

            var xSensBox = new GroupBox
            {
                Text = "X Sensitivity",
                Padding = App.GroupBoxPadding,
                Content = TableLayout.Horizontal(5, new TableCell(xSens, true), new Label { Text = "mm/px", VerticalAlignment = VerticalAlignment.Center })
            };

            var ySens = new TextBox();
            ViewModel.PropertyChanged += (sender, e) => 
            {
                if (e.PropertyName == nameof(ViewModel.Settings))
                {
                    ySens.TextBinding.Convert(
                        s => float.TryParse(s, out var v) ? v : 0,
                        f => f.ToString())
                        .BindDataContext(Binding.Property((MainFormViewModel m) => m.Settings.YSensitivity));
                }
            };

            var ySensBox = new GroupBox
            {
                Text = "Y Sensitivity",
                Padding = App.GroupBoxPadding,
                Content = TableLayout.Horizontal(5, new TableCell(ySens, true), new Label { Text = "mm/px", VerticalAlignment = VerticalAlignment.Center })
            };

            var sensitivityConfig = new StackLayout
            {
                Padding = new Padding(5),
                Spacing = 5,
                Orientation = Orientation.Horizontal,
                Items =
                {
                    new StackLayoutItem(xSensBox, true),
                    new StackLayoutItem(ySensBox, true)
                }
            };
            
            var bindingColumns = 3;
            var bindingRows = 7;
            bindingLayout = new TableLayout(bindingColumns, bindingRows)
            {
                Padding = new Padding(5),
                Spacing = new Size(5, 5)
            };
            for (int i = 0; i < bindingColumns; i++)
                bindingLayout.SetColumnScale(i, true);
            for (int i = 0; i < bindingRows; i++)
                bindingLayout.SetRowScale(i, false);

            filterEditor = new PluginManager<IFilter>();
            filterEditor.GetPluginEnabled = () => App.Settings.Filters.Contains(filterEditor.SelectedPlugin.Path);
            filterEditor.SetPluginEnabled += (sender, enabled) => 
            {
                var path = filterEditor.SelectedPlugin.Path;
                if (enabled && !App.Settings.Filters.Contains(path))
                    App.Settings.Filters.Add(path);
                else if (!enabled && App.Settings.Filters.Contains(path))
                    App.Settings.Filters.Remove(path);
            };

            residentEditor = new PluginManager<IResident>();
            residentEditor.GetPluginEnabled = () => App.Settings.ResidentPlugins.Contains(residentEditor.SelectedPlugin.Path);
            residentEditor.SetPluginEnabled += (sender, enabled) => 
            {
                var path = residentEditor.SelectedPlugin.Path;
                if (enabled && !App.Settings.ResidentPlugins.Contains(path))
                    App.Settings.ResidentPlugins.Add(path);
                else if (!enabled && App.Settings.ResidentPlugins.Contains(path))
                    App.Settings.ResidentPlugins.Remove(path);
            };
            
            // Main Content
            return new TabControl
            {
                Pages = 
                {
                    new TabPage
                    {
                        Text = "Area Configuration",
                        Content = new TableLayout
                        {
                            Rows = 
                            {
                                new TableRow(new TableCell(areaConfig, true))
                                {
                                    ScaleHeight = true
                                },
                                new TableRow(new TableCell())
                            }
                        }
                    },
                    new TabPage
                    {
                        Text = "Sensitivity",
                        Content = sensitivityConfig
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
                        Text = "Plugins",
                        Padding = 5,
                        Content = residentEditor
                    },
                    new TabPage
                    {
                        Text = "Console",
                        Content = new LogView()
                    }
                }
            };
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
            if (AppInfo.PluginDirectory.Exists)
            {
                foreach (var file in AppInfo.PluginDirectory.EnumerateFiles("*.dll", SearchOption.AllDirectories))
                {
                    await App.DriverDaemon.InvokeAsync(d => d.ImportPlugin(file.FullName));
                    await PluginManager.AddPlugin(file);
                }
            }

            if (await App.DriverDaemon.InvokeAsync(d => d.GetTablet()) is TabletProperties tablet)
            {
                SetTabletAreaDimensions(tablet);
            }
            else
            {
                await DetectAllTablets();
            }

            if (await App.DriverDaemon.InvokeAsync(d => d.GetSettings()) is Settings settings)
            {
                ViewModel.Settings = settings;
            }
            else if (AppInfo.SettingsFile.Exists)
            {
                ViewModel.Settings = Settings.Deserialize(AppInfo.SettingsFile);
                await App.DriverDaemon.InvokeAsync(d => d.SetSettings(ViewModel.Settings));
            }
            else
            {
                await ResetSettings();
            }

            UpdateBindingLayout();

            await filterEditor.InitializeAsync();
            await residentEditor.InitializeAsync();

            var virtualScreen = TabletDriverLib.Interop.Platform.VirtualScreen;
            displayAreaEditor.ViewModel.MaxWidth = virtualScreen.Width;
            displayAreaEditor.ViewModel.MaxHeight = virtualScreen.Height;
        }

        private AreaEditor displayAreaEditor, tabletAreaEditor;
        private TableLayout bindingLayout;
        private PluginManager<IFilter> filterEditor;
        private PluginManager<IResident> residentEditor;

        public MainFormViewModel ViewModel
        {
            set => this.DataContext = value;
            get => (MainFormViewModel)this.DataContext;
        }

        private async Task ResetSettings()
        {
            var virtualScreen = TabletDriverLib.Interop.Platform.VirtualScreen;
            var tablet = await App.DriverDaemon.InvokeAsync(d => d.GetTablet());
            ViewModel.Settings = TabletDriverLib.Settings.Defaults;
            ViewModel.Settings.DisplayWidth = virtualScreen.Width;
            ViewModel.Settings.DisplayHeight = virtualScreen.Height;
            ViewModel.Settings.DisplayX = virtualScreen.Width / 2;
            ViewModel.Settings.DisplayY = virtualScreen.Height / 2;
            ViewModel.Settings.TabletWidth = tablet?.Width ?? 0;
            ViewModel.Settings.TabletHeight = tablet?.Height ?? 0;
            ViewModel.Settings.TabletX = tablet?.Width / 2 ?? 0;
            ViewModel.Settings.TabletY = tablet?.Height / 2 ?? 0;

            await App.DriverDaemon.InvokeAsync(d => d.SetSettings(ViewModel.Settings));
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
                        ViewModel.Settings = Settings.Deserialize(file);
                        await App.DriverDaemon.InvokeAsync(d => d.SetSettings(ViewModel.Settings));
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
                    if (ViewModel.Settings is Settings settings)
                    {
                        settings.Serialize(file);
                        await ApplySettings();
                    }
                    break;
            }
        }

        private async Task SaveSettings()
        {
            if (ViewModel.Settings is Settings settings)
            {
                settings.Serialize(AppInfo.SettingsFile);
                await ApplySettings();
            }
        }

        private async Task ApplySettings()
        {
            if (ViewModel.Settings is Settings settings)
                await App.DriverDaemon.InvokeAsync(d => d.SetSettings(settings));
        }

        private async Task DetectAllTablets()
        {
            if (await App.DriverDaemon.InvokeAsync(d => d.DetectTablets()) is TabletProperties tablet)
            {
                var settings = await App.DriverDaemon.InvokeAsync(d => d.GetSettings());
                if (settings != null)
                {
                    await App.DriverDaemon.InvokeAsync(d => d.SetInputHook(settings.AutoHook));
                }
                SetTabletAreaDimensions(tablet);
            }
            else
            {
                Log.Write("Detect", $"Configuration directory '{AppInfo.ConfigurationDirectory.FullName}' does not exist.");
            }
        }

        private void ShowConfigurationEditor()
        {
            var configEditor = new ConfigurationEditor();
            configEditor.Show();
        }

        private void SetTabletAreaDimensions(TabletProperties tablet)
        {
            tabletAreaEditor.ViewModel.MaxWidth = tablet.Width;
            tabletAreaEditor.ViewModel.MaxHeight = tablet.Height;
        }

        private void UpdateBindingLayout()
        {
            // Tip Binding
            var tipBindingControl = new BindingDisplay(ViewModel.Settings.TipButton);
            tipBindingControl.BindingUpdated += (s, binding) => ViewModel.Settings.TipButton = binding;
            var tipBindingGroup = new GroupBox
            {
                Text = "Tip Binding",
                Padding = App.GroupBoxPadding,
                Content = tipBindingControl
            };
            bindingLayout.Add(tipBindingGroup, 0, 0);
            
            var tipPressureControl = new Slider
            {
                MinValue = 0,
                MaxValue = 100,
                Width = 150
            };
            tipPressureControl.BindDataContext(c => c.Value, (MainFormViewModel m) => m.Settings.TipActivationPressure);
            var tipPressureGroup = new GroupBox
            {
                Text = "Tip Activation Pressure",
                Padding = App.GroupBoxPadding,
                Content = tipPressureControl
            };
            bindingLayout.Add(tipPressureGroup, 0, 1);

            // Pen Bindings
            for (int i = 0; i < ViewModel.Settings.PenButtons.Count; i++)
            {
                var penBindingControl = new BindingDisplay(ViewModel.Settings.PenButtons[i])
                {
                    Tag = i
                };
                penBindingControl.BindingUpdated += (sender, binding) => 
                {
                    var index = (int)(sender as BindingDisplay).Tag;
                    ViewModel.Settings.PenButtons[index] = binding;
                };
                var penBindingGroup = new GroupBox
                {
                    Text = $"Pen Button {i + 1}",
                    Padding = App.GroupBoxPadding,
                    Content = penBindingControl
                };
                bindingLayout.Add(penBindingGroup, 1, i);
            }

            // Aux Bindings
            for (int i = 0; i < ViewModel.Settings.AuxButtons.Count; i++)
            {
                var auxBindingControl = new BindingDisplay(ViewModel.Settings.AuxButtons[i])
                {
                    Tag = i
                };
                auxBindingControl.BindingUpdated += (sender, binding) =>
                {
                    int index = (int)(sender as BindingDisplay).Tag;
                    ViewModel.Settings.AuxButtons[index] = binding;
                };
                var auxBindingGroup = new GroupBox
                {
                    Text = $"Express Key {i + 1}",
                    Padding = App.GroupBoxPadding,
                    Content = auxBindingControl
                };
                bindingLayout.Add(auxBindingGroup, 2, i);
            }
        }

        private void ShowTabletDebugger()
        {
            var debugger = new TabletDebugger();
            debugger.Show();
        }
    }
}
