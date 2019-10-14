using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Avalonia.Controls;
using OpenTabletDriverGUI.Models;
using ReactiveUI;
using TabletDriverLib;
using TabletDriverLib.Class;
using TabletDriverLib.Tools.Display;

namespace OpenTabletDriverGUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Trace.Listeners.Add(TraceListener);
        }

        public Settings Settings
        {
            set => this.RaiseAndSetIfChanged(ref _settings, value);
            get => _settings;
        }
        private Settings _settings;
        
        [XmlIgnore]
        private ReactiveTraceListener TraceListener
        {
            set => this.RaiseAndSetIfChanged(ref _trace, value);
            get => _trace;
        }
        private ReactiveTraceListener _trace = new ReactiveTraceListener();

        [XmlIgnore]
        private Driver Driver
        {
            set => this.RaiseAndSetIfChanged(ref _driver, value);
            get => _driver;
        }
        private Driver _driver;

        [XmlIgnore]
        public bool InputHooked 
        {
            private set => this.RaiseAndSetIfChanged(ref _hooked, value);
            get => _hooked;
        }
        private bool _hooked;

        [XmlIgnore]
        private IDisplay Display
        {
            set => this.RaiseAndSetIfChanged(ref _disp, value);
            get => _disp;
        }
        private IDisplay _disp;

        [XmlIgnore]
        private float FullTabletWidth
        {
            set => this.RaiseAndSetIfChanged(ref _fTabW, value);
            get => _fTabW;
        }
        private float _fTabW;

        [XmlIgnore]
        private float FullTabletHeight
        {
            set => this.RaiseAndSetIfChanged(ref _fTabH, value);
            get => _fTabH;
        }
        private float _fTabH;


        private void OpenConfigurations(DirectoryInfo directory)
        {
            List<FileInfo> configRepository = directory.EnumerateFiles().ToList();
            foreach (var dir in directory.EnumerateDirectories())
                configRepository.AddRange(dir.EnumerateFiles());

            var configs = configRepository.ConvertAll(file => TabletProperties.Read(file));
            OpenConfigurations(configs);
        }

        private void OpenConfigurations(IEnumerable<TabletProperties> configs)
        {
            foreach (var config in configs)
            {
                if (Driver.InputManager.OpenTablet(config))
                    break;
            }

            if (Driver.InputManager.Tablet == null)
            {
                Log.Fail("No configured tablets connected.");
            }
        }

        public void UpdateSettings()
        {
            Driver.InputManager.DisplayArea = new Area
            {
                Width = Settings.DisplayWidth,
                Height = Settings.DisplayHeight,
                Position = new Point(Settings.DisplayX, Settings.DisplayY),
                Rotation = Settings.DisplayRotation
            };
            Log.Info($"Set display area: " + Driver.InputManager.DisplayArea);
            Driver.InputManager.TabletArea = new Area
            {
                Width = Settings.TabletWidth,
                Height = Settings.TabletHeight,
                Position = new Point(Settings.TabletX, Settings.TabletY),
                Rotation = Settings.TabletRotation
            };
            Log.Info($"Set tablet area:  " + Driver.InputManager.TabletArea);
            Log.Info("Applied all settings.");
        }

        public void Initialize()
        {
            Driver = new Driver();
            Driver.InputManager.TabletSuccessfullyOpened += (sender, tablet) => 
            {
                FullTabletWidth = tablet.Width;
                FullTabletHeight = tablet.Height;
            };

            SetPlatformSpecifics(Environment.OSVersion.Platform);
            
            var settings = new FileInfo("settings.xml");
            if (settings.Exists)
            {
                Settings = Settings.Deserialize(settings);
                Log.Info("Loaded user settings");
            }
            else
            {
                UseDefaultSettings();
            }
            
            var configurationDir = new DirectoryInfo("Configurations");
            if (configurationDir.Exists)
                OpenConfigurations(configurationDir);
        }

        private void SetPlatformSpecifics(PlatformID platform)
        {
            switch (platform)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.WinCE:
                    Display = new WindowsDisplay();
                    return;
                case PlatformID.Unix:
                    Display = new XDisplay();
                    return;
                case PlatformID.MacOSX:
                    Display = new MacOSDisplay();
                    return;
                default:
                    return;
            }
        }

        public void UseDefaultSettings()
        {
            Settings = new Settings();
            if (Driver.InputManager.Tablet != null)
            {
                Settings.TabletWidth = Driver.InputManager.TabletProperties.Width;
                Settings.TabletHeight = Driver.InputManager.TabletProperties.Height;
                Settings.TabletX = 0;
                Settings.TabletY = 0;
            }

            Settings.DisplayWidth = Display.Width;
            Settings.DisplayHeight = Display.Height;
            Settings.DisplayX = 0;
            Settings.DisplayY = 0;

            UpdateSettings();
        }

        public async Task LoadTabletConfiguration()
        {
            var result = await new OpenFileDialog().ShowAsync(App.Current.MainWindow);
            if (result != null)
            {
                var files = result.ToList().ConvertAll(item => new FileInfo(item));
                var configs = files.ConvertAll(file => TabletProperties.Read(file));
                OpenConfigurations(configs);
            }
        }

        public async Task SaveTabletConfiguration()
        {
            var result = await new SaveFileDialog().ShowAsync(App.Current.MainWindow);
            if (result != null)
            {
                var file = new FileInfo(result);
                Driver.InputManager.TabletProperties.Write(file);
                Log.Info($"Saved current tablet configuration to '{file.FullName}'.");
            }
        }

        public async Task OpenTabletConfigurationFolder()
        {
            var path = await new OpenFolderDialog().ShowAsync(App.Current.MainWindow);
            if (path != null)
            {
                var directory = new DirectoryInfo(path);
                if (directory.Exists)
                    OpenConfigurations(directory);
            }
        }

        public async Task LoadSettingsDialog()
        {
            var path = await new OpenFileDialog().ShowAsync(App.Current.MainWindow);
            if (path != null)
            {
                var file = new FileInfo(path[0]);
                try
                {
                    Settings = Settings.Deserialize(file);
                    Log.Info("Successfully read settings from file.");
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                    Log.Fail("Unable to read settings from file: " + path[0]);
                }
            }
        }

        public async Task SaveSettingsDialog()
        {
            var path = await new SaveFileDialog().ShowAsync(App.Current.MainWindow);
            if (path != null)
            {
                var file = new FileInfo(path);
                try 
                {
                    Settings.Serialize(file);
                    Log.Info("Wrote settings to file: " + path);
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                    Log.Fail("Unable to write settings to file: " + path);
                }
            }
        }

        public void ToggleHook()
        {
            try
            {
                InputHooked = !InputHooked;
                Log.Info("Hooking inputs: " + InputHooked);
                Driver.InputManager.BindPositions(InputHooked);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                Log.Fail("Unable to hook input.");
                InputHooked = !InputHooked;
            }
        }
    }
}
