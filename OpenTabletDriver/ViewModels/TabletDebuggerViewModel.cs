using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HidSharp;
using ReactiveUI;
using TabletDriverLib;
using TabletDriverLib.Tablet;

namespace OpenTabletDriver.ViewModels
{
    public class TabletDebuggerViewModel : ViewModelBase, IDisposable
    {
        public TabletDebuggerViewModel(DeviceReader<IDeviceReport> tabletReader, DeviceReader<IDeviceReport> auxReader)
        {
            _tReader = tabletReader;
            _tReader.Report += HandleTabletReport;
            _auxReader = auxReader;
            _auxReader.Report += HandleAuxReport;
        }

        private DeviceReader<IDeviceReport> _tReader, _auxReader;

        public void Dispose()
        {
            _tReader.Report -= HandleTabletReport;
            _auxReader.Report -= HandleAuxReport;
        }

        private void HandleTabletReport(object sender, IDeviceReport report)
        {
            RawTabletReport = report.StringFormat(true);
            var fmt = report.StringFormat(false);
            TabletProperties = fmt.Split(", ", StringSplitOptions.None).ToObservableCollection();
        }

        private void HandleAuxReport(object sender, IDeviceReport report)
        {
            RawAuxReport = report.StringFormat(true);
            var fmt = report.StringFormat(false);
            AuxProperties = fmt.Split(", ", StringSplitOptions.None).ToObservableCollection();
        }

        private string _traw, _auxRaw;

        public string RawTabletReport
        {
            set => this.RaiseAndSetIfChanged(ref _traw, value);
            get => _traw;
        }

        public string RawAuxReport
        {
            set => this.RaiseAndSetIfChanged(ref _auxRaw, value);
            get => _auxRaw;
        }

        private ObservableCollection<string> _tprops = new ObservableCollection<string>();
        private ObservableCollection<string> _auxprops = new ObservableCollection<string>();

        public ObservableCollection<string> TabletProperties
        {
            set => this.RaiseAndSetIfChanged(ref _tprops, value);
            get => _tprops;
        }

        public ObservableCollection<string> AuxProperties
        {
            set => this.RaiseAndSetIfChanged(ref _auxprops, value);
            get => _auxprops;
        }
    }
}