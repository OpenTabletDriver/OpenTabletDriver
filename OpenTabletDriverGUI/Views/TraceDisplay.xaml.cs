using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using OpenTabletDriverGUI.Models;

namespace OpenTabletDriverGUI.Views
{
    public class TraceDisplay : UserControl
    {
        public TraceDisplay()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public static readonly StyledProperty<ReactiveTraceListener> TraceListenerProperty =
        AvaloniaProperty.Register<TraceDisplay, ReactiveTraceListener>(nameof(TraceListener));
        
        public ReactiveTraceListener TraceListener
        {
            set => SetValue(TraceListenerProperty, value);
            get => GetValue(TraceListenerProperty);
        }
    }
}