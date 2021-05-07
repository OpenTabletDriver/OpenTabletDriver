using System;
using System.Linq;
using System.Threading.Tasks;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Plugin;
using static System.Console;

namespace OpenTabletDriver.Console
{
    partial class Program
    {
        static async Task<Settings> GetSettings()
        {
            return await Driver.Instance.GetSettings();
        }

        static async Task<Profile> GetProfile(int id)
        {
            return await Driver.Instance.GetProfile(new TabletHandlerID { Value = id });
        }

        static async Task ApplySettings(Settings settings)
        {
            await Driver.Instance.SetSettings(settings);
        }

        static async Task ApplyProfile(int id, Profile profile)
        {
            await Driver.Instance.SetProfile(new TabletHandlerID { Value = id }, profile);
        }

        static async Task ModifySettings(Action<Settings> func)
        {
            var settings = await GetSettings();
            func.Invoke(settings);
            await ApplySettings(settings);
        }

        static async Task ModifyProfile(int handle, Action<Profile> func)
        {
            var profile = await GetProfile(handle);
            func.Invoke(profile);
            await ApplyProfile(handle, profile);
        }

        static async Task ListTypes<T>()
        {
            var types = from plugin in AppInfo.PluginManager.GetChildTypes<T>()
                select AppInfo.PluginManager.GetPluginReference(plugin);
            foreach (var type in types)
            {
                var output = string.IsNullOrWhiteSpace(type.Name) ? type.Path : $"{type.Path} [{type.Name}]";
                await Out.WriteLineAsync(output);
            }
        }
    }
}
