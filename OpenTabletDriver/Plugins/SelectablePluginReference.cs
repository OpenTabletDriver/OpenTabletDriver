using ReactiveUI;

namespace OpenTabletDriver.Plugins
{
    public class SelectablePluginReference : PluginReference
    {
        public SelectablePluginReference(string path, bool isEnabled) : base(path)
        {
            IsEnabled = isEnabled;
        }

        public SelectablePluginReference() : base()
        {
            IsEnabled = false;
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
            get => _isEnabled;
        }
    }
}