using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TabletDriverLib;
using TabletDriverPlugin.Tablet;

namespace OpenTabletDriver.Windows
{
    public class TabletDebugger : Window
    {
        public TabletDebugger(params DeviceReader<IDeviceReport>[] deviceReaders) : this()
        {
            DataContext = new TabletDebuggerViewModel(deviceReaders);
        }
        
        public TabletDebugger()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}