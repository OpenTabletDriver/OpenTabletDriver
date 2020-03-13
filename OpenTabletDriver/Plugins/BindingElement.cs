using ReactiveUI;

namespace OpenTabletDriver.Plugins
{
    public class BindingElement : ReactiveObject
    {
        private string _path;
        public string Path
        {
            set => this.RaiseAndSetIfChanged(ref _path, value);
            get => _path;
        }

        private string _value;
        public string Value
        {
            set => this.RaiseAndSetIfChanged(ref _value, value);
            get => _value;
        }
    }
}