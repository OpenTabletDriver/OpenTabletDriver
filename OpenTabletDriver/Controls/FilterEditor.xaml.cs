using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using OpenTabletDriver.Models;
using OpenTabletDriver.Plugins;
using OpenTabletDriver.Tools;
using TabletDriverLib;
using TabletDriverPlugin;
using TabletDriverPlugin.Attributes;
using TabletDriverPlugin.Tablet;

namespace OpenTabletDriver.Controls
{
    public class FilterEditor : UserControl, INotifyPropertyChanged, IViewModelRoot<FilterEditorViewModel>
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