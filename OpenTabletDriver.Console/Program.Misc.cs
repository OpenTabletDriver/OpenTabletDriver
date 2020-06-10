using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;
using TabletDriverLib;
using TabletDriverLib.Plugins;
using static System.Console;

namespace OpenTabletDriver.Console
{
    partial class Program
    {
        static async Task<Settings> GetSettings()
        {
            return await DriverDaemon.InvokeAsync(d => d.GetSettings());
        }

        static async Task ApplySettings(Settings settings)
        {
            await DriverDaemon.InvokeAsync(d => d.SetSettings(settings));
        }

        static async Task ModifySettings(Action<Settings> func)
        {
            var settings = await GetSettings();
            func.Invoke(settings);
            await ApplySettings(settings);
        }

        static async Task ListTypes<T>()
        {
            var types = from path in await DriverDaemon.InvokeAsync(d => d.GetChildTypes<T>())
                select new PluginReference(path);
            foreach (var type in types)
            {
                var output = string.IsNullOrWhiteSpace(type.Name) ? type.Path : $"{type.Path} [{type.Name}]";
                await Out.WriteLineAsync(output);
            }
        }
    }
}