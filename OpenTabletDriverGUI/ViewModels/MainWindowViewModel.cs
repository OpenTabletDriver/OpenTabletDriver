using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Avalonia.Controls;
using OpenTabletDriverGUI.Models;
using OpenTabletDriverGUI.Views;
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
            set
            {
                this.RaiseAndSetIfChanged(ref _settings, value);
                UpdateSettings();
            }
            get => _settings;
        }
        private Settings _settings;
        
        private ReactiveTraceListener TraceListener
        {
            set => this.RaiseAndSetIfChanged(ref _trace, value);
            get => _trace;
        }
        private ReactiveTraceListener _trace = new ReactiveTraceListener();

        private Driver Driver
        {
            set => this.RaiseAndSetIfChanged(ref _driver, value);
            get => _driver;
        }
        private Driver _driver;

        public bool InputHooked 
        {
            private set => this.RaiseAndSetIfChanged(ref _hooked, value);
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

        private void OpenConfigurations(DirectoryInfo directory)
        {
            List<FileInfo> configRepository = directory.EnumerateFiles().ToList();
            foreach (var dir in directory.EnumerateDirectories())
                configRepository.AddRange(dir.EnumerateFiles());

            Tablets = configRepository.ConvertAll(file => TabletProperties.Read(file)).ToObservableCollection();
            Driver.OpenTablet(Tablets);
        }

        public void UpdateSettings()
        {
            Driver.DisplayArea = new Area
            {
                Width = Settings.DisplayWidth,
                Height = Settings.DisplayHeight,
                Position = new Point(Settings.DisplayX, Settings.DisplayY),
                Rotation = Settings.DisplayRotation
            };
            Log.Info($"Set display area: " + Driver.DisplayArea);
            Driver.TabletArea = new Area
            {
                Width = Settings.TabletWidth,
                Height = Settings.TabletHeight,
                Position = new Point(Settings.TabletX, Settings.TabletY),
                Rotation = Settings.TabletRotation
            };
            Log.Info($"Set tablet area:  " + Driver.TabletArea);
            Log.Info("Applied all settings.");
        }

        public void Initialize()
        {
            Driver = new Driver();
            Driver.TabletSuccessfullyOpened += (sender, tablet) => 
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
                UpdateSettings();
            }
        }

        public void DetectTablet()
        {
            Driver.OpenTablet(Tablets);
        }

        public async Task OpenConfigurationManager()
        {
            var cfgMgr = new ConfigurationManager()
            {
                DataContext = new ConfigurationManagerViewModel()
                {
                    Configurations = Tablets
                }
            };
            await cfgMgr.ShowDialog(App.Current.MainWindow);
            Driver.OpenTablet(Tablets);
        }

        public async Task OpenTabletConfigurationFolder()
        {
            var fd = new OpenFolderDialog();
            var path = await fd.ShowAsync(App.Current.MainWindow);
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
            var result = await fd.ShowAsync(App.Current.MainWindow);
            if (result != null)
            {
                var file = new FileInfo(result[0]);
                try
                {
                    Settings = Settings.Deserialize(file);
                    Log.Info("Successfully read settings from file.");
                }
                catch (Exception ex)
                {
                    Log.Exception(ex);
                    Log.Fail("Unable to read settings from file: " + result[0]);
                }
            }
        }

        public async Task SaveSettingsDialog()
        {
            var fd = FileDialogs.CreateSaveFileDialog("Saving settings", "XML Document", "xml");
            var path = await fd.ShowAsync(App.Current.MainWindow);
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
                Driver.BindInput(InputHooked);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                Log.Fail("Unable to hook input.");
                InputHooked = !InputHooked;
            }
        }

        private void SetTheme(string name)
        {
            Settings.Theme = name;
            Log.Info($"Using theme '{name}'.");
            (App.Current as App).Restart(this);
        }
    }
}
