using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenTabletDriver.Controls
{
    public class ResidentPluginEditor : UserControl, IViewModelRoot<ResidentPluginEditorViewModel>
    {
        public ResidentPluginEditor()
        {
            InitializeComponent();
            ViewModel = new ResidentPluginEditorViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public ResidentPluginEditorViewModel ViewModel
        {
            set => DataContext = value;
            get => (ResidentPluginEditorViewModel)DataContext;
        }
    }
}