using System.Collections.Generic;
using System.CommandLine;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Console
{
    public static class Extensions
    {
        public static void AddRange(this Command command, IEnumerable<Symbol> symbols)
        {
            foreach (var sym in symbols)
                command.Add(sym);
        }

        public static string GetFormattedBinding(this PluginSettingStore store)
        {
            var pluginRef = store.GetPluginReference();
            var binding = pluginRef.Construct<IBinding>();

            return $"{pluginRef.Name}: {binding.Property}";
        }

        public static IEnumerable<string> GetFormattedBindings(this PluginSettingStoreCollection bindings)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                var str = bindings[i].GetFormattedBinding();
                yield return $"[{i}, {str}]";
            }
        }
    }
}