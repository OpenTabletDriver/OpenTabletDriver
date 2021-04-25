namespace OpenTabletDriver.Desktop.Reflection
{
    public class StoredPluginReference : PluginReference
    {
        public StoredPluginReference(PluginManager pluginManager, PluginSettingStore store)
            : base(pluginManager, store.Path)
        {
            PluginSettings = store;
        }

        private PluginSettingStore PluginSettings { get; }

        public override T Construct<T>()
        {
            var value = base.Construct<T>();
            PluginSettings.ApplySettings(value);
            return value;
        }

        public override T Construct<T>(params object[] args)
        {
            var value = base.Construct<T>(args);
            PluginSettings.ApplySettings(value);
            return value;
        }
    }
}