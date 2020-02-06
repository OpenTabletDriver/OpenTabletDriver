using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using OpenTabletDriver.Models;
using OpenTabletDriver.Plugins;
using OpenTabletDriver.Tools;
using OpenTabletDriver.Windows;
using ReactiveUI;
using TabletDriverLib;
using TabletDriverPlugin.Attributes;
using TabletDriverPlugin.Tablet;

namespace OpenTabletDriver.Controls
{
    public class FilterEditorViewModel : ViewModelBase
    {
        public FilterEditorViewModel()
        {
            RefreshPlugins();
        }

        public Settings Settings
        {
            get
            {
                var lifetime = App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                return (lifetime.MainWindow?.DataContext as MainWindowViewModel)?.Settings;
            }
        }

        public void RefreshPlugins()
        {
            var plugins = from filter in PluginManager.GetChildTypes<IFilter>()
                where !filter.IsInterface
                where !filter.GetCustomAttributes(false).Any(a => a is PluginIgnoreAttribute)
                select filter;

            var filters = new Collection<SelectablePluginReference>();
            foreach (var plugin in plugins)
            {
                bool isEnabled = Settings?.Filters.Any(f => f == plugin.FullName) ?? false;
                filters.Add(new SelectablePluginReference(plugin.FullName, isEnabled));
            }

            Filters = new ObservableCollection<SelectablePluginReference>(filters);
        }

        public IEnumerable<IFilter> ConstructEnabledFilters()
        {
            var filters = from filter in Filters
                where filter.IsEnabled
                select filter.Construct<IFilter>();

            var filterList = new List<IFilter>(filters);
            
            for (int i = 0; i < filterList.Count(); i++)
            {
                var filter = filterList[i];
                PluginTools.SetPluginSettings(filter, Settings.PluginSettings);
                filterList[i] = filter;
            }

            return filterList;
        }

        private void UpdateSettings(IFilter filter)
        {
            if (filter is IFilter)
            {
                var settings = PluginTools.GetPluginSettings(filter);
                foreach (var pair in settings)
                {
                    if (Settings.PluginSettings.ContainsKey(pair.Item1))
                        Settings.PluginSettings[pair.Item1] = pair.Item2;
                    else
                        Settings.PluginSettings.Add(pair.Item1, pair.Item2);
                }
            }
        }

        private ObservableCollection<SelectablePluginReference> filters;
        public ObservableCollection<SelectablePluginReference> Filters
        {
            set => this.RaiseAndSetIfChanged(ref filters, value);
            get => filters;
        }

        private SelectablePluginReference _selectedPlugin;
        public SelectablePluginReference SelectedPlugin
        {
            set
            {
                SelectedFilterTemplate = value?.Construct<IFilter>();
                PluginTools.SetPluginSettings(SelectedFilterTemplate, Settings.PluginSettings);
                this.RaiseAndSetIfChanged(ref _selectedPlugin, value);
            }
            get => _selectedPlugin;
        }

        private IFilter _filterTemplate;
        public IFilter SelectedFilterTemplate
        {
            set
            {
                if (value is INotifyPropertyChanged inotify)
                    inotify.PropertyChanged += (s, e) => UpdateSettings(value);
                
                var filterSettings = PropertyTools.GetPropertyControls(value, nameof(SelectedFilterTemplate), Settings.PluginSettings);
                SelectedFilterSettings = new ObservableCollection<IControl>(filterSettings);
                
                this.RaiseAndSetIfChanged(ref _filterTemplate, value);
            }
            get => _filterTemplate;
        }

        private ObservableCollection<IControl> _selectedFilterSettings;
        public ObservableCollection<IControl> SelectedFilterSettings
        {
            set => this.RaiseAndSetIfChanged(ref _selectedFilterSettings, value);
            get => _selectedFilterSettings;
        }
    }
}