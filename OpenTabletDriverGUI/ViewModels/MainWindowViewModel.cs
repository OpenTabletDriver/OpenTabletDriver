using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using OpenTabletDriverGUI.Models;
using ReactiveUI;
using TabletDriverLib;
using TabletDriverLib.Class;

namespace OpenTabletDriverGUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Trace.Listeners.Add(TraceListener);
            Driver = new Driver();
        }
        
        private ReactiveTraceListener _trace = new ReactiveTraceListener();
        public ReactiveTraceListener TraceListener
        {
            set => this.RaiseAndSetIfChanged(ref _trace, value);
            get => _trace;
        }

        private Driver _driver;
        public Driver Driver
        {
            set => this.RaiseAndSetIfChanged(ref _driver, value);
            get => _driver;
        }

        private float _dW, _dH, _dX, _dY, _dR, _tW, _tH, _tX, _tY, _tR;

        public float DisplayWidth 
        {
            set => this.RaiseAndSetIfChanged(ref _dW, value);
            get => _dW;
        }

        public float DisplayHeight
        {
            set => this.RaiseAndSetIfChanged(ref _dH, value);
            get => _dH;
        }

        public float DisplayX
        {
            set => this.RaiseAndSetIfChanged(ref _dX, value);
            get => _dX;
        }

        public float DisplayY
        {
            set => this.RaiseAndSetIfChanged(ref _dY, value);
            get => _dY;
        }

        public float DisplayRotation
        {
            set => this.RaiseAndSetIfChanged(ref _dR, value);
            get => _dR;
        }

        public float TabletWidth
        {
            set => this.RaiseAndSetIfChanged(ref _tW, value);
            get => _tW;
        }

        public float TabletHeight
        {
            set => this.RaiseAndSetIfChanged(ref _tH, value);
            get => _tH;
        }

        public float TabletX
        {
            set => this.RaiseAndSetIfChanged(ref _tX, value);
            get => _tX;
        }

        public float TabletY
        {
            set => this.RaiseAndSetIfChanged(ref _tY, value);
            get => _tY;
        }

        public float TabletRotation
        {
            set => this.RaiseAndSetIfChanged(ref _tR, value);
            get => _tR;
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
            Log.Info($"Set display area: [{DisplayWidth}x{DisplayHeight}@{DisplayX},{DisplayY}:{DisplayRotation}]");
            Driver.InputManager.TabletArea = new Area
            {
                Width = TabletWidth,
                Height = TabletHeight,
                Position = new Point(TabletX, TabletY),
                Rotation = TabletRotation
            };
            Log.Info($"Set tablet area:  [{TabletWidth}x{TabletHeight}@{TabletX},{TabletY}:{TabletRotation}]");
            Log.Info("Applied all settings.");
        }

        public async Task LoadTabletConfiguration()
        {
            var fd = new OpenFileDialog();
            var result = await fd.ShowAsync(App.Current.MainWindow);
            if (result != null)
            {
                foreach (var file in result)
                {
                    try
                    {
                        var tablet = TabletProperties.Read(new FileInfo(file));
                        if (Driver.InputManager.OpenTablet(tablet))
                            break;
                    }
                    catch (Exception ex)
                    {
                        Log.WriteException(ex);
                    }
                }
            }
        }
    }
}
