using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenTabletDriver.Controls
{
    public class FilterEditor : UserControl, IViewModelRoot<FilterEditorViewModel>
    {
        public FilterEditor()
        {
            InitializeComponent();
            ViewModel = new FilterEditorViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public FilterEditorViewModel ViewModel
        {
            set => this.DataContext = value;
            get => (FilterEditorViewModel)this.DataContext;
        }
    }
}