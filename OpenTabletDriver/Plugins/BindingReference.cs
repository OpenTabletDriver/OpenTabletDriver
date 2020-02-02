using ReactiveUI;

namespace OpenTabletDriver.Plugins
{
    public class BindingReference : PluginReference
    {
        public BindingReference(string full) : base()
        {
            var tokens = full.Split(", ", 2);
            Path = tokens[0];
            Value = tokens[1];
        }

        private string _value;
        public string Value
        {
            set => this.RaiseAndSetIfChanged(ref _value, value);
            get => _value;
        }

        public override string ToString()
        {
            return Name + ", " + Value;
        }
    }
}