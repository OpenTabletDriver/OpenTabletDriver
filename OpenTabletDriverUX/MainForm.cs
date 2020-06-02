using Eto.Forms;
using Eto.Drawing;
using OpenTabletDriverUX.Controls;
using System.IO;
using TabletDriverPlugin.Tablet;
using System.Threading.Tasks;
using TabletDriverLib;
using TabletDriverPlugin;
using OpenTabletDriverUX.Windows;

namespace OpenTabletDriverUX
{
    public partial class MainForm : Form, IViewModelRoot<MainFormViewModel>
    {
        public MainForm()
        {
            this.DataContext = new MainFormViewModel();
            
            Title = "OpenTabletDriver";
            ClientSize = new Size(960, 730);
            MinimumSize = new Size(960, 730);
            Icon = App.Logo.WithSize(App.Logo.Size);

            displayAreaEditor = new AreaEditor();
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
                Padding = new Padding(5),
                Content = displayAreaEditor
            };

            tabletAreaEditor = new AreaEditor(true);
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
                Padding = new Padding(5),
                Content = tabletAreaEditor
            };
            
            var outputConfigContent = new TableLayout
            {
                Padding = new Padding(5),
                Rows = 
                {
                    new TableRow(new TableCell(displayAreaGroup, true))
                    {
                        ScaleHeight = true
                    },
                    new TableRow(new TableCell(tabletAreaGroup, true))
                    {
                        ScaleHeight = true
                    }
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
            
            // Main Content
            Content = new TabControl
            {
                Pages = 
                {
                    // Main Tab
                    new TabPage
                    {
                        Text = "Output Configuration",
                        // Content = mainTabLayout
                        Content = outputConfigContent
                    },
                    new TabPage
                    {
                        Text = "Bindings",
                        Content = bindingLayout
                    },
                    new TabPage
                    {
                        Text = "Filters",
                        Content = new StackLayout
                        {
                        }
                    },
                    new TabPage
                    {
                        Text = "Plugins",
                        Content = new StackLayout
                        {
                        }
                    },
                    new TabPage
                    {
                        Text = "Console",
                        Content = new TableLayout
                        {
                        }
                    }
                }
            };

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
            // TODO: Show tablet debugger

            var configurationEditor = new Command { MenuText = "Open Configuration Editor...", Shortcut = Application.Instance.CommonModifier | Keys.E };
            configurationEditor.Executed += (sender, e) => ShowConfigurationEditor();

            // Menu
            Menu = new MenuBar
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

            InitializeAsync();
        }

        public async void InitializeAsync()
        {
            if (AppInfo.PluginDirectory.Exists)
            {
                foreach (var file in AppInfo.PluginDirectory.EnumerateFiles("*.dll", SearchOption.AllDirectories))
                {
                    await App.DriverDaemon.InvokeAsync(d => d.ImportPlugin(file.FullName));
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

            var virtualScreen = TabletDriverLib.Interop.Platform.VirtualScreen;
            displayAreaEditor.ViewModel.MaxWidth = virtualScreen.Width;
            displayAreaEditor.ViewModel.MaxHeight = virtualScreen.Height;
        }

        private AreaEditor displayAreaEditor, tabletAreaEditor;
        private TableLayout bindingLayout;

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
                    if (await App.DriverDaemon.InvokeAsync(d => d.GetSettings()) is Settings settings)
                        settings.Serialize(file);
                    break;
            }
        }

        private async Task SaveSettings()
        {
            if (await App.DriverDaemon.InvokeAsync(d => d.GetSettings()) is Settings settings)
                settings.Serialize(AppInfo.SettingsFile);
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

        public void ShowConfigurationEditor()
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
                Padding = new Padding(5),
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
                Padding = new Padding(5),
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
                    Padding = new Padding(5),
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
                    Padding = new Padding(5),
                    Content = auxBindingControl
                };
                bindingLayout.Add(auxBindingGroup, 2, i);
            }
        }
    }
}
