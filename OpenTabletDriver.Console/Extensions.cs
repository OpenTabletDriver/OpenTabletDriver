using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.Console
{
    public static class Extensions
    {
        public static void AddCommands(this Command command, IEnumerable<Command> commands)
        {
            foreach (var addedCommand in commands)
                command.Add(addedCommand);
        }

        [return: NotNullIfNotNull("setting")]
        public static string? Format(this PluginSetting? setting)
        {
            if (setting == null)
                return null;

            return $"{{ {setting.Property}: {setting.GetValue(typeof(object))} }}";
        }

        public static string? Format(this PluginSettingStore? store)
        {
            if (store is not { Enable: true })
                return null;

            var storeSettings = new List<string>();
            foreach (var setting in store.Settings)
                storeSettings.Add(setting.Format());

            string prefix = store.Name ?? store.Path;
            string? suffix = storeSettings.Count == 0 ? null : string.Join(", ", storeSettings);

            return string.IsNullOrEmpty(suffix) ? $"'{prefix}'" : $"'{prefix}: {suffix}'";
        }

        public static IEnumerable<string> Format(this IEnumerable<PluginSettingStore?> storeCollection, bool showIndex = false)
        {
            var nonNullStoreCollection = storeCollection
                .Where(store => store is not null)
                .Cast<PluginSettingStore>()
                .ToList();

            if (nonNullStoreCollection.Count > 0)
            {
                int index = 0;
                bool empty = true;
                foreach (var store in nonNullStoreCollection)
                {
                    var str = store.Format();
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        empty = false;

                        if (showIndex)
                            yield return $"[{index}]: {str}";
                        else if (store.Enable)
                            yield return store.Format()!; // suppress null-check as it's known non-null if store is enabled
                        // else nothing if disabled
                    }
                    index++;
                }
                if (empty)
                    yield return "None";
            }
            else
            {
                yield return "None";
            }
        }
    }
}
