using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using OpenTabletDriver.Models;
using OpenTabletDriver.Views;
using ReactiveUI;
using TabletDriverLib;
using TabletDriverLib.Interop;
using TabletDriverLib.Interop.Cursor;
using TabletDriverLib.Interop.Display;
using TabletDriverLib.Output;
using TabletDriverPlugin;
using TabletDriverPlugin.Logging;
using TabletDriverPlugin.Tablet;

namespace OpenTabletDriver.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public void Initialize()
        {
            // Start logging
            Log.Output += (sender, message) =>
            {
                Dispatcher.UIThread.Post(() => Messages.Add(message));
                StatusMessage = message;
            };

            // Create new instance of the driver
            Driver = new Driver();
            Driver.TabletSuccessfullyOpened += (sender, tablet) => 
            {
                FullTabletWidth = tablet.Width;
                FullTabletHeight = tablet.Height;
                
                if (Settings.TabletWidth == 0 && Settings.TabletHeight == 0)
                {
                    Settings.TabletWidth = tablet.Width;
                    Settings.TabletHeight = tablet.Height;
                    Settings.TabletX = tablet.Width / 2;
                    Settings.TabletY = tablet.Height / 2;
                }

                Driver.BindingEnabled = InputHooked;
                ApplySettings();
            };

            // Use platform specific virtual screen
            VirtualScreen = Platform.VirtualScreen;
            
            var settingsPath = Path.Join(Program.SettingsDirectory.FullName, "settings.xml");
            var settings = new FileInfo(settingsPath);
            if (settings.Exists)
            {
                Settings = Settings.Deserialize(settings);
                Log.Write("Settings", $"Loaded saved user settings from '{settingsPath}'");
            } 
            else
            {
                Defaults();
                Log.Write("Settings", "Using default settings.");
            }

            Log.Write("Settings", $"Configuration directory is '{Program.ConfigurationDirectory.FullName}'.");
            Log.Write("Settings", $"Plugin directory is '{Program.PluginDirectory.FullName}'");
            LoadPlugins(Program.PluginDirectory);
            InitializePlugins();

            // Find tablet configurations and try to open a tablet
            if (Program.ConfigurationDirectory.Exists)
                OpenConfigurations(Program.ConfigurationDirectory);
            else
                Tablets = new ObservableCollection<TabletProperties>();
        }

        private async void InitializePlugins()
        {
            while (BackgroundTaskActive)
                await Task.Delay(100);

            var outputModes = from mode in PluginManager.GetChildTypes<IOutputMode>()
                where !mode.IsInterface
                select mode.FullName;
            OutputModes = new ObservableCollection<string>(outputModes);

            var filters = from filter in PluginManager.GetChildTypes<IFilter>()
                where !filter.IsInterface
                select filter.FullName;
            Filters = new ObservableCollection<string>(filters);
            Filters.Insert(0, "{Disable}");
        }

        #region Bindable Properties

        private Settings _settings;
        private LogMessage _status;
        private Driver _driver;
        private bool _hooked, _debugging, _rawReports;
        public IVirtualScreen _scr;
        private IDisplay _disp;
        private float _fTabW, _fTabH;
        private bool _pluginsLoading = true;
        private ObservableCollection<LogMessage> _messages = new ObservableCollection<LogMessage>();
        private ObservableCollection<TabletProperties> _tablets;
        private ObservableCollection<string> _filters, _outputs;
        
        public Settings Settings
        {
            set => this.RaiseAndSetIfChanged(ref _settings, value);
            get => _settings;
        }
        
        public ObservableCollection<LogMessage> Messages
        {
            set => this.RaiseAndSetIfChanged(ref _messages, value);
            get => _messages;
        }

        public LogMessage StatusMessage
        {
            set => this.RaiseAndSetIfChanged(ref _status, value);
            get => _status;
        }

        private Driver Driver
        {
            set => this.RaiseAndSetIfChanged(ref _driver, value);
            get => _driver;
        }

        public bool InputHooked 
        {
            private set
            {
                Driver.BindingEnabled = value;
                this.RaiseAndSetIfChanged(ref _hooked, value);
            }
            get => _hooked;
        }

        public IVirtualScreen VirtualScreen
        {
            set => this.RaiseAndSetIfChanged(ref _scr, value);
            get => _scr;
        }

        private IDisplay SelectedDisplay
        {
            set
            {
                this.RaiseAndSetIfChanged(ref _disp, value);
                SelectDisplay(SelectedDisplay);
            }
            get => _disp;
        }

        private float FullTabletWidth
        {
            set => this.RaiseAndSetIfChanged(ref _fTabW, value);
            get => _fTabW;
        }

        private float FullTabletHeight
        {
            set => this.RaiseAndSetIfChanged(ref _fTabH, value);
            get => _fTabH;
        }

        public ObservableCollection<TabletProperties> Tablets
        {
            set => this.RaiseAndSetIfChanged(ref _tablets, value);
            get => _tablets;
        }

        public bool Debugging
        {
            set
            {
                this.RaiseAndSetIfChanged(ref _debugging, value);
                Driver.Debugging = value;
            } 
            get => _debugging;
        }

        public bool RawReports
        {
            set
            {
                this.RaiseAndSetIfChanged(ref _rawReports, value);
                Driver.RawReports = value;
            }
            get => _rawReports;
        }

        public bool BackgroundTaskActive
        {
            set => this.RaiseAndSetIfChanged(ref _pluginsLoading, value);
            get => _pluginsLoading;
        }

        public ObservableCollection<string> OutputModes
        {
            set => this.RaiseAndSetIfChanged(ref _outputs, value);
            get => _outputs;
        }

        public ObservableCollection<string> Filters
        {
            set => this.RaiseAndSetIfChanged(ref _filters, value);
            get => _filters;
        }

        #endregion

        #region Buttons

        public async Task ShowAbout()
        {
            var about = new About();
            await about.ShowDialog(App.MainWindow);
        }

        public void DetectTablet()
        {
            Driver.Open(Tablets);
            if (Settings.AutoHook && Driver.Tablet != null)
                InputHooked = true;
        }

        private void OpenConfigurations(DirectoryInfo directory)
        {
            List<FileInfo> configRepository = directory.EnumerateFiles().ToList();
            foreach (var dir in directory.EnumerateDirectories())
                configRepository.AddRange(dir.EnumerateFiles());

            Tablets = configRepository.ConvertAll(file => TabletProperties.Read(file)).ToObservableCollection();
            DetectTablet();
        }

        public async void ApplySettings()
        {
            while (BackgroundTaskActive)
                await Task.Delay(100);
            
            Driver.OutputMode = PluginManager.ConstructObject<IOutputMode>(Settings.OutputMode);
            
            if (Driver.OutputMode is IOutputMode mode)  
            {
                Log.Write("Settings", $"Using output mode '{Driver.OutputMode.GetType().FullName}'");
                mode.Filter = PluginManager.ConstructObject<IFilter>(Settings.ActiveFilterName);
                if (mode.Filter != null)
                    Log.Write("Settings", $"Using filter '{mode.Filter.GetType().FullName}'.");
                else if (string.IsNullOrWhiteSpace(Settings.ActiveFilterName))
                    Log.Write("Settings", $"No filter selected.");
                else 
                    Log.Write("Settings", $"Failed to get filter '{Settings.ActiveFilterName}'.", true);
                
                mode.TabletProperties = Driver.TabletProperties;
            }
            else if (string.IsNullOrWhiteSpace(Settings.OutputMode))
            {
                Log.Write("Settings", $"Error: No output mode has been selected.", true);
            }
            else
            {
                Log.Write("Settings", $"Error: Failed to get output mode '{Settings.OutputMode}'.", true);
            }

            if (Driver.OutputMode is IAbsoluteMode absolute)
            {
                absolute.Output = new Area
                {
                    Width = Settings.DisplayWidth,
                    Height = Settings.DisplayHeight,
                    Position = new Point(Settings.DisplayX, Settings.DisplayY),
                    Rotation = Settings.DisplayRotation
                };
                Log.Write("Settings", $"Set display area: " + absolute.Output);
                
                absolute.Input = new Area
                {
                    Width = Settings.TabletWidth,
                    Height = Settings.TabletHeight,
                    Position = new Point(Settings.TabletX, Settings.TabletY),
                    Rotation = Settings.TabletRotation
                };
                Log.Write("Settings", $"Set tablet area:  " + absolute.Input);
                
                absolute.AreaClipping = Settings.EnableClipping;
                Log.Write("Settings", "Clipping is " + (absolute.AreaClipping ? "enabled" : "disabled"));
            }

            if (Driver.OutputMode is IBindingHandler<MouseButton> bindingHandler)
            {
                bindingHandler.TipBinding = Settings.TipButton;
                bindingHandler.TipActivationPressure = Settings.TipActivationPressure;
                Log.Write("Settings", $"Tip Binding: '{bindingHandler.TipBinding}'@{bindingHandler.TipActivationPressure}%");

                if (Settings.PenButtons != null)
                {
                    for (int index = 0; index < Settings.PenButtons.Count; index++)
                        bindingHandler.PenButtonBindings[index] = Settings.PenButtons[index];

                    Log.Write("Settings", $"Pen Bindings: " + String.Join(", ", bindingHandler.PenButtonBindings));
                }
                if (Settings.AuxButtons != null)
                {
                    for (int index = 0; index < Settings.AuxButtons.Count; index++)
                        bindingHandler.AuxButtonBindings[index] = Settings.AuxButtons[index];

                    Log.Write("Settings", $"Express Key Bindings: " + String.Join(", ", bindingHandler.AuxButtonBindings));
                }
            }

            Log.Write("Settings", "Applied all settings.");
        }

        public void UseDefaultSettings()
        {
            Defaults();
            SetTheme(Settings.Theme);
        }

        private void Defaults()
        {
            Settings = new Settings()
            {
                DisplayWidth = VirtualScreen.Width,
                DisplayHeight = VirtualScreen.Height,
                DisplayX = VirtualScreen.Width / 2,
                DisplayY = VirtualScreen.Height / 2,
                PenButtons = new ObservableCollection<MouseButton>(new MouseButton[2]),
                AuxButtons = new ObservableCollection<MouseButton>(new MouseButton[4])
            };

            if (Driver.Tablet != null)
            {
                Settings.TabletWidth = Driver.TabletProperties.Width;
                Settings.TabletHeight = Driver.TabletProperties.Height;
                Settings.TabletX = Driver.TabletProperties.Width / 2;
                Settings.TabletY = Driver.TabletProperties.Height / 2;
            }

            ResetWindowSize();
            ApplySettings();
        }

        public void OpenTabletDebugger()
        {
            var debugger = new TabletDebugger
            {
                DataContext = new TabletDebuggerViewModel(Driver.TabletReader, Driver.AuxReader)
            };
            debugger.Show();
        }

        public async Task OpenConfigurationManager()
        {
            var cfgMgr = new ConfigurationManager()
            {
                DataContext = new ConfigurationManagerViewModel()
                {
                    Configurations = Tablets,
                    Devices = Driver.Devices.ToList().ToObservableCollection()
                }
            };
            await cfgMgr.ShowDialog(App.MainWindow);
            DetectTablet();
        }

        public async Task OpenTabletConfigurationFolder()
        {
            var fd = new OpenFolderDialog();
            var path = await fd.ShowAsync(App.MainWindow);
            if (path != null)
            {
                var directory = new DirectoryInfo(path);
                if (directory.Exists)
                    OpenConfigurations(directory);
            }
        }

        public async Task LoadSettingsDialog()
        {
            var fd = FileDialogs.CreateOpenFileDialog("Open settings", "XML Document", "xml");
            var result = await fd.ShowAsync(App.MainWindow);
            if (result != null)
            {
                var file = new FileInfo(result[0]);
                Load(file);
            }
        }

        public async Task SaveSettingsDialog()
        {
            var fd = FileDialogs.CreateSaveFileDialog("Saving settings", "XML Document", "xml");
            var path = await fd.ShowAsync(App.MainWindow);
            if (path != null)
            {
                var file = new FileInfo(path);
                Save(file);
            }
        }

        public void SaveSettings()
        {
            ApplySettings();
            if (!Program.SettingsDirectory.Exists)
                Program.SettingsDirectory.Create();
            var settingsPath = Path.Join(Program.SettingsDirectory.FullName, "settings.xml");
            var file = new FileInfo(settingsPath);
            Save(file);
        }

        private bool Load(FileInfo file)
        {
            try
            {
                Settings = Settings.Deserialize(file);
                Log.Write("Settings", $"Read settings from '{file.FullName}'.");
                ApplySettings();
                SetTheme(Settings.Theme);
                return true;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                Log.Write("Settings", $"Unable to read settings from file '{file.FullName}'", true);
                return false;
            }
        }

        private bool Save(FileInfo file)
        {
            try
            {
                Settings.Serialize(file);
                Log.Write("Settings", $"Saved settings to '{file.FullName}'.");
                return true;
            }
            catch (Exception ex)
            {
                if (Debugging)
                    Log.Exception(ex);
                Log.Write("Settings", $"Failed to write settings to '{file.FullName}'.", true);
                return false;
            }
        }

        public async void LoadPlugins(DirectoryInfo directory)
        {
            BackgroundTaskActive = true;
            if (!directory.Exists)
                directory.Create();
            
            var files = directory.GetFiles();
            foreach (var plugin in files)
            {
                if (await PluginManager.AddPlugin(plugin))
                    Log.Write("Plugin", $"Loaded plugin '{plugin.Name}'.");
            }
            BackgroundTaskActive = false;
        }

        public void ToggleHook()
        {
            try
            {
                InputHooked = !InputHooked;
                Log.Write("Driver", $"Driver is {(InputHooked ? "enabled" : "disabled")}.");
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                Log.Write("Driver", "Unable to toggle input hook.", true);
            }
        }

        public void ResetWindowSize()
        {
            Settings.WindowWidth = 1280;
            Settings.WindowHeight = 720;
        }

        private void SetTheme(string name)
        {
            Settings.Theme = name;
            Log.Write("Theme", $"Using theme '{name}'.");
            App.Restart(this);
        }

        private void SetMode(string name)
        {
            Settings.OutputMode = name;
            ApplySettings();
        }

        private void SelectDisplay(IDisplay display)
        {
            Settings.DisplayWidth = display.Width;
            Settings.DisplayHeight = display.Height;
            if (display is IVirtualScreen virtualScreen)
            {
                Settings.DisplayX = virtualScreen.Width / 2;
                Settings.DisplayY = virtualScreen.Height / 2;
            }
            else
            {
                Settings.DisplayX = display.Position.X + (display.Width / 2) + VirtualScreen.Position.X;
                Settings.DisplayY = display.Position.Y + (display.Height / 2) + VirtualScreen.Position.Y;
            }
        }

        #endregion
    }
}
