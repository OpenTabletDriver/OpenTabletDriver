using TabletDriverLib.Plugins;

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

        public string Value { get; set; }

        public override string ToString()
        {
            return Name + ", " + Value;
        }
    }
}