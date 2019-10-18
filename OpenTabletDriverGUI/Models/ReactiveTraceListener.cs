using System;
using System.ComponentModel;
using System.Diagnostics;
using ReactiveUI;

namespace OpenTabletDriverGUI.Models
{
    public class ReactiveTraceListener : TraceListener, IReactiveObject
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event ReactiveUI.PropertyChangingEventHandler PropertyChanging;

        private string _t, _l;
        public string Contents 
        {
            private set => this.RaiseAndSetIfChanged(ref _t, value);
            get => _t;
        }

        public string Status
        {
            private set => this.RaiseAndSetIfChanged(ref _l, value);
            get => _l;
        }

        public event EventHandler<string> StatusChanged;

        public void RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        public void RaisePropertyChanging(ReactiveUI.PropertyChangingEventArgs args)
        {
            PropertyChanging?.Invoke(this, args);
        }

        public override void Write(string message)
        {
            Contents += message;
            Status = message;
            StatusChanged?.Invoke(this, message);
        }

        public override void WriteLine(string message)
        {
            if (string.IsNullOrWhiteSpace(Contents))
                Contents += message;
            else
                Contents += Environment.NewLine + message;
            Status = message;
            StatusChanged?.Invoke(this, message);
        }
    }
}