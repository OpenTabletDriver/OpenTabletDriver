using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TabletDriverLib.Component;

namespace OpenTabletDriverGUI.Views
{
    public class LogView : UserControl
    {
        public LogView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static readonly StyledProperty<IEnumerable<LogMessage>> MessagesProperty =
            AvaloniaProperty.Register<LogView, IEnumerable<LogMessage>>(nameof(Messages));
        
        public IEnumerable<LogMessage> Messages
        {
            set => SetValue(MessagesProperty, value);
            get => GetValue(MessagesProperty);
        }
    }
}