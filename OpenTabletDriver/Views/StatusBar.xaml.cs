using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TabletDriverLib;
using TabletDriverLib.Component;

namespace OpenTabletDriver.Views
{
    public class StatusBar : UserControl
    {
        public StatusBar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public static readonly StyledProperty<LogMessage> CurrentStatusProperty =
        AvaloniaProperty.Register<StatusBar, LogMessage>(nameof(CurrentStatus));
        
        public LogMessage CurrentStatus
        {
            set => SetValue(CurrentStatusProperty, value);
            get => GetValue(CurrentStatusProperty);
        }
    }
}