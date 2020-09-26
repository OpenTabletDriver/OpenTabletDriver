using System;
using System.Linq;
using System.Threading.Tasks;
using OpenTabletDriver.Reflection;
using static System.Console;

namespace OpenTabletDriver.Console
{
    partial class Program
    {
        static async Task<Settings> GetSettings()
        {
            return await Driver.Instance.GetSettings();
        }

        static async Task ApplySettings(Settings settings)
        {
            await Driver.Instance.SetSettings(settings);
        }

        static async Task ModifySettings(Action<Settings> func)
        {
            var settings = await GetSettings();
            func.Invoke(settings);
            await ApplySettings(settings);
        }

        static async Task ListTypes<T>()
        {
            var types = from plugin in PluginManager.GetChildTypes<T>()
                select new PluginReference(plugin);
            foreach (var type in types)
            {
                var output = string.IsNullOrWhiteSpace(type.Name) ? type.Path : $"{type.Path} [{type.Name}]";
                await Out.WriteLineAsync(output);
            }
        }
    }
}