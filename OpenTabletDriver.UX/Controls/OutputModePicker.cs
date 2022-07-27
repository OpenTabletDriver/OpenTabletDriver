using System.Collections.Immutable;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Output;

namespace OpenTabletDriver.UX.Controls
{
    public class OutputModePicker : DropDown
    {
        private readonly IPluginFactory _pluginFactory;

        public OutputModePicker(App app, IPluginFactory pluginFactory)
        {
            _pluginFactory = pluginFactory;

            ItemTextBinding = Binding.Property<Type, string>(t => t.GetFriendlyName() ?? t.GetFullyQualifiedName());
            Refresh();

            app.PluginManager.AssembliesChanged += (_, _) => Refresh();
        }

        protected override IEnumerable<object> CreateDefaultDataStore()
        {
            return _pluginFactory.GetMatchingTypes(typeof(IOutputMode)).ToImmutableArray();
        }

        private void Refresh() => Application.Instance.AsyncInvoke(() =>
        {
            DataStore = CreateDefaultDataStore();
        });
    }
}
