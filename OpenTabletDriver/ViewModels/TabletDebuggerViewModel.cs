using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HidSharp;
using ReactiveUI;
using TabletDriverLib;
using TabletDriverLib.Tablet;

namespace OpenTabletDriver.ViewModels
{
    public class TabletDebuggerViewModel : ViewModelBase, IDisposable
    {
        public TabletDebuggerViewModel(params DeviceReader<IDeviceReport>[] deviceReaders)
        {
            _readers = from r in deviceReaders
                where r != null
                select r;

            foreach (var reader in _readers)
            {
                reader.Report += HandleReport;
            }
        }

        private IEnumerable<DeviceReader<IDeviceReport>> _readers;

        public void Dispose()
        {
            foreach (var reader in _readers)
            {
                reader.Report -= HandleReport;
            }
        }

        private void HandleReport(object sender, IDeviceReport report)
        {
            if (report is ITabletReport tabletReport)
            {
                RawTabletReport = tabletReport.StringFormat(true);
                TabletProperties = tabletReport.StringFormat(false).Split(", ", StringSplitOptions.None).ToObservableCollection();
            }
            if (report is IAuxReport auxReport)
            {
                RawAuxReport = auxReport.StringFormat(true);
                AuxProperties = auxReport.StringFormat(false).Split(", ", StringSplitOptions.None).ToObservableCollection();
            }
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