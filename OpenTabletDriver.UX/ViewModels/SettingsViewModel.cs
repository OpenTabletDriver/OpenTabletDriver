using System.Reflection;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.UX.ViewModels
{
    public class SettingsViewModel : NotifyPropertyChanged
    {
        public SettingsViewModel(PluginSettings settings, TypeInfo type)
        {
            Settings = settings;
            Type = type;
        }

        public PluginSettings Settings { get; }
        public TypeInfo Type { get; }
    }
}
