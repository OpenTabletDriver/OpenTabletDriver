using System.Collections.Generic;
using System.CommandLine;
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

        public static string Format(this PluginSetting setting)
        {
            if (setting == null)
                return null;

            return $"{{ {setting.Property}: {setting.GetValue(typeof(object))} }}";
        }

        public static string Format(this PluginSettingStore store)
        {
            if (store == null || !store.Enable)
                return null;

            IList<string> storeSettings = new List<string>();
            foreach (var setting in store.Settings)
                storeSettings.Add(setting.Format());

            string prefix = store.Name ?? store.Path;
            string suffix = storeSettings.Count == 0 ? null : string.Join(", ", storeSettings);

            return string.IsNullOrEmpty(suffix) ? $"'{prefix}'" : $"'{prefix}: {suffix}'";
        }

        public static IEnumerable<string> Format(this IEnumerable<PluginSettingStore> storeCollection, bool showIndex = false)
        {
            if (storeCollection.Any(s => s != null))
            {
                int index = 0;
                bool empty = true;
                foreach (var store in storeCollection)
                {
                    var str = store.Format();
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        empty = false;
                        yield return showIndex ? $"[{index}]: {str}" : store.Format();
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
