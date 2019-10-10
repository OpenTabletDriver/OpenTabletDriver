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
    [XmlRoot("Configuration", DataType = "OpenTabletDriverCfg")]
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Trace.Listeners.Add(TraceListener);
        }
        
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

        private float _dW, _dH, _dX, _dY, _dR, _tW, _tH, _tX, _tY, _tR;

        [XmlElement("DisplayWidth")]
        public float DisplayWidth 
        {
            set => this.RaiseAndSetIfChanged(ref _dW, value);
            get => _dW;
        }

        [XmlElement("DisplayHeight")]
        public float DisplayHeight
        {
            set => this.RaiseAndSetIfChanged(ref _dH, value);
            get => _dH;
        }

        [XmlElement("DisplayXOffset")]
        public float DisplayX
        {
            set => this.RaiseAndSetIfChanged(ref _dX, value);
            get => _dX;
        }

        [XmlElement("DisplayYOffset")]
        public float DisplayY
        {
            set => this.RaiseAndSetIfChanged(ref _dY, value);
            get => _dY;
        }

        [XmlElement("DisplayRotation")]
        public float DisplayRotation
        {
            set => this.RaiseAndSetIfChanged(ref _dR, value);
            get => _dR;
        }

        [XmlElement("TabletWidth")]
        public float TabletWidth
        {
            set => this.RaiseAndSetIfChanged(ref _tW, value);
            get => _tW;
        }

        [XmlElement("TabletHeight")]
        public float TabletHeight
        {
            set => this.RaiseAndSetIfChanged(ref _tH, value);
            get => _tH;
        }

        [XmlElement("TabletXOffset")]
        public float TabletX
        {
            set => this.RaiseAndSetIfChanged(ref _tX, value);
            get => _tX;
        }

        [XmlElement("TabletYOffset")]
        public float TabletY
        {
            set => this.RaiseAndSetIfChanged(ref _tY, value);
            get => _tY;
        }

        [XmlElement("TabletRotation")]
        public float TabletRotation
        {
            set => this.RaiseAndSetIfChanged(ref _tR, value);
            get => _tR;
        }

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
                Width = DisplayWidth,
                Height = DisplayHeight,
                Position = new Point(DisplayX, DisplayY),
                Rotation = DisplayRotation
            };
            Log.Info($"Set display area: [{DisplayWidth}x{DisplayHeight}@{DisplayX},{DisplayY};{DisplayRotation}°]");
            Driver.InputManager.TabletArea = new Area
            {
                Width = TabletWidth,
                Height = TabletHeight,
                Position = new Point(TabletX, TabletY),
                Rotation = TabletRotation
            };
            Log.Info($"Set tablet area:  [{TabletWidth}x{TabletHeight}@{TabletX},{TabletY};{TabletRotation}°]");
            Log.Info("Applied all settings.");
        }

        public void Initialize()
        {
            Driver = new Driver();
            SetPlatformSpecifics(Environment.OSVersion.Platform);
            
            var settings = new FileInfo("settings.xml");
            if (settings.Exists)
            {
                Deserialize(settings);
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
            if (Driver.InputManager.Tablet != null)
            {
                TabletWidth = Driver.InputManager.TabletProperties.Width;
                TabletHeight = Driver.InputManager.TabletProperties.Height;
                TabletX = 0;
                TabletY = 0;
            }

            DisplayWidth = Display.Width;
            DisplayHeight = Display.Height;
            DisplayX = 0;
            DisplayY = 0;

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
                    Deserialize(file);
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
                    Serialize(file);
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

        #region XML Serialization

        private static readonly XmlSerializer XmlSerializer = new XmlSerializer(typeof(MainWindowViewModel));

        public void Deserialize(FileInfo file)
        {
            using (var stream = file.OpenRead())
            {
                var data = (MainWindowViewModel)XmlSerializer.Deserialize(stream);
                data.CopyPropertiesTo(this);
                UpdateSettings();
            }
        }

        public void Serialize(FileInfo file)
        {
            using (var stream = file.OpenWrite())
                XmlSerializer.Serialize(stream, this);
        }

        #endregion
    }
}
