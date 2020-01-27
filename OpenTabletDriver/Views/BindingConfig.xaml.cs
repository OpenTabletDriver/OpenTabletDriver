using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using OpenTabletDriver.ViewModels;

namespace OpenTabletDriver.Views
{
    public class BindingConfig : Window
    {
        public BindingConfig(string binding)
        {
            DataContext = new BindingConfigViewModel(binding);
            InitializeComponent();
        }

        public BindingConfig()
        {
            DataContext = new BindingConfigViewModel();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
            // Focus keybinding setup on tab change
            var tabctrl = this.Find<TabControl>("TabCtrl");
            var keybindTab = this.Find<TabItem>("KeybindTab");
            var keybindCtrl = this.Find<Border>("KeybindCtrl");
            tabctrl.SelectionChanged += (a, b) =>
            {
                if (b.AddedItems.Contains(keybindTab))
                    keybindCtrl.Child.Focus();
            };
        }

        public string Binding
        {
            set => ViewModel.Binding = value;
            get => ViewModel.Binding;
        }

        public BindingConfigViewModel ViewModel
        {
            set => DataContext = value;
            get => (BindingConfigViewModel)DataContext;
        }
    }
}