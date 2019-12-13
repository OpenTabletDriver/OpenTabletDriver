using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using OpenTabletDriverGUI.Models;
using OpenTabletDriverGUI.Views;
using ReactiveUI;
using TabletDriverLib;
using TabletDriverLib.Component;
using TabletDriverLib.Interop;
using TabletDriverLib.Interop.Cursor;
using TabletDriverLib.Interop.Display;
using TabletDriverLib.Output;
using TabletDriverLib.Tablet;

namespace OpenTabletDriverGUI.ViewModels
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
                Driver.BindingEnabled = InputHooked;
            };

            // Use platform specific display
            Display = Platform.Display;
            
            var settingsPath = Path.Join(Program.SettingsDirectory.FullName, "settings.xml");
            var settings = new FileInfo(settingsPath);
            if (settings.Exists)
            {
                Settings = Settings.Deserialize(settings);
                Log.Write("Settings", $"Loaded user settings from '{settingsPath}'");
            } 
            else
            {
                Defaults();
            }

            // Find tablet configurations and try to open a tablet
            Log.Write("Settings", $"Configuration directory is '{Program.ConfigurationDirectory.FullName}'.");
            if (Program.ConfigurationDirectory.Exists)
                OpenConfigurations(Program.ConfigurationDirectory);
            else
                Tablets = new ObservableCollection<TabletProperties>();

            ApplySettings();
        }

        #region Bindable Properties

        public Settings Settings
        {
            set => this.RaiseAndSetIfChanged(ref _settings, value);
            get => _settings;
        }
        private Settings _settings;
        
        public ObservableCollection<LogMessage> Messages
        {
            set => this.RaiseAndSetIfChanged(ref _messages, value);
            get => _messages;
        }
        private ObservableCollection<LogMessage> _messages = new ObservableCollection<LogMessage>();

        public LogMessage StatusMessage
        {
            set => this.RaiseAndSetIfChanged(ref _status, value);
            get => _status;
        }
        private LogMessage _status;

        private Driver Driver
        {
            set => this.RaiseAndSetIfChanged(ref _driver, value);
            get => _driver;
        }
        private Driver _driver;

        public bool InputHooked 
        {
            private set
            {
                Driver.BindingEnabled = value;
                this.RaiseAndSetIfChanged(ref _hooked, value);
            }
            get => _hooked;
        }
        private bool _hooked;

        private IDisplay Display
        {
            set => this.RaiseAndSetIfChanged(ref _disp, value);
            get => _disp;
        }
        private IDisplay _disp;

        private float FullTabletWidth
        {
            set => this.RaiseAndSetIfChanged(ref _fTabW, value);
            get => _fTabW;
        }
        private float _fTabW;

        private float FullTabletHeight
        {
            set => this.RaiseAndSetIfChanged(ref _fTabH, value);
            get => _fTabH;
        }
        private float _fTabH;

        public ObservableCollection<TabletProperties> Tablets
        {
            set => this.RaiseAndSetIfChanged(ref _tablets, value);
            get => _tablets;
        }
        private ObservableCollection<TabletProperties> _tablets;

        public bool Debugging
        {
            set
            {
                this.RaiseAndSetIfChanged(ref _debugging, value);
                Driver.Debugging = value;
            } 
            get => _debugging;
        }
        private bool _debugging;

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

        public void ApplySettings()
        {
            switch (Settings.OutputMode)
            {
                case "Absolute":
                    Driver.OutputMode = new AbsoluteMode();
                    break;
            }
            Log.Write("Settings", $"Using output mode '{Settings.OutputMode}'");

            if (Driver.OutputMode is OutputMode outputMode)
            {
                outputMode.TabletProperties = Driver.TabletProperties;
            }
            if (Driver.OutputMode is AbsoluteMode absolute)
            {
                absolute.DisplayArea = new Area
                {
                    Width = Settings.DisplayWidth,
                    Height = Settings.DisplayHeight,
                    Position = new Point(Settings.DisplayX, Settings.DisplayY),
                    Rotation = Settings.DisplayRotation
                };
                Log.Write("Settings", $"Set display area: " + absolute.DisplayArea);
                
                absolute.TabletArea = new Area
                {
                    Width = Settings.TabletWidth,
                    Height = Settings.TabletHeight,
                    Position = new Point(Settings.TabletX, Settings.TabletY),
                    Rotation = Settings.TabletRotation
                };
                Log.Write("Settings", $"Set tablet area:  " + absolute.TabletArea);
                
                absolute.Clipping = Settings.EnableClipping;
                Log.Write("Settings", "Clipping is " + (absolute.Clipping ? "enabled" : "disabled"));
                
                absolute.TipBinding = Settings.TipButton;
                absolute.TipActivationPressure = Settings.TipActivationPressure;
                absolute.TipEnabled = absolute.TipBinding != MouseButton.None;
                Log.Write("Settings", $"Tip Binding: '{absolute.TipBinding}'@{absolute.TipActivationPressure}%");

                if (Settings.PenButtons != null)
                {
                    for (int index = 0; index < Settings.PenButtons.Length; index++)
                        absolute.PenButtonBindings[index] = Settings.PenButtons[index];

                    Log.Write("Settings", $"Pen Bindings: " + String.Join(", ", absolute.PenButtonBindings));
                }
                if (Settings.AuxButtons != null)
                {
                    for (int index = 0; index < Settings.AuxButtons.Length; index++)
                        absolute.AuxButtonBindings[index] = Settings.AuxButtons[index];

                    Log.Write("Settings", $"Express Key Bindings: " + String.Join(", ", absolute.AuxButtonBindings));
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
                DisplayWidth = Display.Width,
                DisplayHeight = Display.Height,
                DisplayX = 0,
                DisplayY = 0
            };

            if (Driver.Tablet != null)
            {
                Settings.TabletWidth = Driver.TabletProperties.Width;
                Settings.TabletHeight = Driver.TabletProperties.Height;
                Settings.TabletX = 0;
                Settings.TabletY = 0;
            }

            ApplySettings();
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
                try
                {
                    Settings = Settings.Deserialize(file);
                    Log.Write("Settings", "Successfully read settings from file.");
                    ApplySettings();
                    SetTheme(Settings.Theme);
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                    Log.Write("Settings", "Unable to read settings from file: " + result[0], true);
                }
            }
        }

        public async Task SaveSettingsDialog()
        {
            var fd = FileDialogs.CreateSaveFileDialog("Saving settings", "XML Document", "xml");
            var path = await fd.ShowAsync(App.MainWindow);
            if (path != null)
            {
                var file = new FileInfo(path);
                try 
                {
                    Settings.Serialize(file);
                    Log.Write("Settings", "Wrote settings to file: " + path);
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                    Log.Write("Settings", "Unable to write settings to file: " + path);
                }
            }
        }

        public void SaveSettings()
        {
            if (!Program.SettingsDirectory.Exists)
                Program.SettingsDirectory.Create();
            var settingsPath = Path.Join(Program.SettingsDirectory.FullName, "settings.xml");
            var settings = new FileInfo(settingsPath);
            try
            {
                Settings.Serialize(settings);
                Log.Write("Settings", $"Saved settings to '{settingsPath}'.");
            }
            catch (Exception ex)
            {
                if (Debugging)
                    Log.Exception(ex);
                Log.Write("Settings", $"Failed to write settings to '{settingsPath}'.", true);
            }
        }

        public void ToggleHook()
        {
            try
            {
                InputHooked = !InputHooked;
                Log.Write("Driver", $"Input hook is {(InputHooked ? "enabled" : "disabled")}.");
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                Log.Write("Driver", "Unable to toggle input hook.", true);
            }
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

        #endregion
    }
}
