using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using OpenTabletDriver.Plugins;
using OpenTabletDriver.Tools;
using OpenTabletDriver.Windows;
using ReactiveUI;
using TabletDriverLib;
using TabletDriverPlugin.Attributes;

namespace OpenTabletDriver.Controls
{
    public abstract class PluginSettingsEditorViewModel<T> : ViewModelBase where T : class
    {
        public PluginSettingsEditorViewModel()
        {
            Refresh();
        }

        protected Settings Settings
        {
            get
            {
                var lifetime = App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                return (lifetime.MainWindow?.DataContext as MainWindowViewModel)?.Settings;
            }
        }

        public abstract void Refresh();

        public IEnumerable<T> ConstructEnabledPlugins()
        {
            var plugins = from plugin in Plugins
                where plugin.IsEnabled
                select plugin.Construct<T>();
            
            var pluginList = new List<T>(plugins);

            for (int i = 0; i < pluginList.Count(); i++)
            {
                var plugin = pluginList[i];
                PluginTools.SetPluginSettings(plugin, Settings.PluginSettings);
                pluginList[i] = plugin;
            }

            return pluginList;
        }

        private void UpdateSettings(T value)
        {
            if (value is T)
            {
                var settings = PluginTools.GetPluginSettings(value);
                foreach (var pair in settings)
                {
                    if (Settings.PluginSettings.ContainsKey(pair.Item1))
                        Settings.PluginSettings[pair.Item1] = pair.Item2;
                    else
                        Settings.PluginSettings.Add(pair.Item1, pair.Item2);
                }
            }
        }

        private ObservableCollection<SelectablePluginReference> _plugins;
        public ObservableCollection<SelectablePluginReference> Plugins
        {
            set => this.RaiseAndSetIfChanged(ref _plugins, value);
            get => _plugins;
        }

        private SelectablePluginReference _selectedPlugin;
        public SelectablePluginReference SelectedPlugin
        {
            set
            {
                SelectedPluginTemplate = value?.Construct<T>();
                PluginTools.SetPluginSettings(SelectedPluginTemplate, Settings.PluginSettings);
                this.RaiseAndSetIfChanged(ref _selectedPlugin, value);
            }
            get => _selectedPlugin;
        }

        private T _template;
        public T SelectedPluginTemplate
        {
            set
            {
                if (value is INotifyPropertyChanged inotify)
                    inotify.PropertyChanged += (s, e) => UpdateSettings(value);

                var pluginSettings = PropertyTools.GetPropertyControls(value, nameof(SelectedPluginTemplate), Settings.PluginSettings);
                SelectedPluginSettings = new ObservableCollection<IControl>(pluginSettings);

                this.RaiseAndSetIfChanged(ref _template, value);
            }
            get => _template;
        }

        private ObservableCollection<IControl> _selectedPluginSettings;
        public ObservableCollection<IControl> SelectedPluginSettings
        {
            set => this.RaiseAndSetIfChanged(ref _selectedPluginSettings, value);
            get => _selectedPluginSettings;
        }
    }
}