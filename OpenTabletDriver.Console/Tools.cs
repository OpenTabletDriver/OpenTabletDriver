using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.CommandLine;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Console
{
    public static class Tools
    {
        public static void AddRange(this Command command, IEnumerable<Symbol> symbols)
        {
            foreach (var sym in symbols)
                command.Add(sym);
        }

        public static string GetFormattedBinding(PluginSettingStore store)
        {
            var pluginRef = store.GetPluginReference();
            var binding = pluginRef.Construct<IBinding>();

            return $"{pluginRef.Name}: {binding.Property}";
        }

        public static IEnumerable<string> GetFormattedBindings(PluginSettingStoreCollection bindings)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                var str = GetFormattedBinding(bindings[i]);
                yield return $"[{i}, {str}]";
            }
        }
    }
}