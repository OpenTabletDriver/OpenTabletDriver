using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;
using static System.Console;

namespace OpenTabletDriver.Console
{
    partial class Program
    {
        #region I/O

        private static async Task LoadSettings(FileInfo file)
        {
            var settings = Serialization.Deserialize<Settings>(file);
            await ApplySettings(settings);
        }

        private static async Task SaveSettings(FileInfo file)
        {
            var settings = await GetSettings();
            Serialization.Serialize(file, settings);
        }

        private static async Task LoadProfile(int handle, FileInfo file)
        {
            var profile = ProfileSerializer.Deserialize(file);
            await ApplyProfile(handle, profile);
        }

        private static async Task SaveProfile(int handle, FileInfo file)
        {
            var profile = await GetProfile(handle);
            ProfileSerializer.Serialize(profile, file);
        }

        #endregion

        #region Modify Settings

        private static async Task SetDisplayArea(int handle, float width, float height, float x, float y)
        {
            await ModifyProfile(handle, p =>
            {
                p.DisplayWidth = width;
                p.DisplayHeight = height;
                p.DisplayX = x;
                p.DisplayY = y;
            });
        }

        private static async Task SetTabletArea(int handle, float width, float height, float x, float y, float rotation = 0)
        {
            await ModifyProfile(handle, p =>
            {
                p.TabletWidth = width;
                p.TabletHeight = height;
                p.TabletX = x;
                p.TabletY = y;
                p.TabletRotation = rotation;
            });
        }

        private static async Task SetSensitivity(int handle, float xSens, float ySens, float rotation = 0)
        {
            await ModifyProfile(handle, p =>
            {
                p.XSensitivity = xSens;
                p.YSensitivity = ySens;
                p.RelativeRotation = rotation;
            });
        }

        private static async Task SetResetTime(int handle, int ms)
        {
            await ModifyProfile(handle, p => p.ResetTime = TimeSpan.FromMilliseconds(ms));
        }

        private static async Task SetTipBinding(int handle, string name, float threshold)
        {
            await ModifyProfile(handle, p =>
            {
                var tipBinding = AppInfo.PluginManager.ConstructObject<IBinding>(name);

                p.TipButton = new PluginSettingStore(tipBinding);
                p.TipActivationPressure = threshold;
            });
        }

        private static async Task SetPenBinding(int handle, string name, int index)
        {
            await ModifyProfile(handle, p =>
            {
                var binding = AppInfo.PluginManager.ConstructObject<IBinding>(name);

                p.PenButtons[index] = new PluginSettingStore(binding);
            });
        }

        private static async Task SetAuxBinding(int handle, string name, int index)
        {
            await ModifyProfile(handle, p =>
            {
                var binding = AppInfo.PluginManager.ConstructObject<IBinding>(name);

                p.AuxButtons[index] = new PluginSettingStore(binding);
            });
        }

        private static async Task SetAutoHook(int handle, bool isEnabled)
        {
            await ModifyProfile(handle, p => p.AutoHook = isEnabled);
        }

        private static async Task SetEnableClipping(int handle, bool isEnabled)
        {
            await ModifyProfile(handle, p => p.EnableClipping = isEnabled);
        }

        private static async Task SetEnableAreaLimiting(int handle, bool isEnabled)
        {
            await ModifyProfile(handle, p => p.EnableAreaLimiting = isEnabled);
        }

        private static async Task SetLockAspectRatio(int handle, bool isEnabled)
        {
            await ModifyProfile(handle, p => p.LockAspectRatio = isEnabled);
        }

        private static async Task SetOutputMode(int handle, string mode)
        {
            await ModifyProfile(handle, p => p.OutputMode = new PluginSettingStore(mode));
        }

        private static async Task SetFilters(int handle, params string[] filters)
        {
            await ModifyProfile(handle, p =>
            {
                var collection = new PluginSettingStoreCollection();
                foreach (var path in filters)
                    collection.Add(new PluginSettingStore(path));

                p.Filters = collection;
            });
        }

        private static async Task SetTools(params string[] tools)
        {
            await ModifySettings(s =>
            {
                var collection = new PluginSettingStoreCollection();
                foreach (var path in tools)
                    collection.Add(new PluginSettingStore(path));

                s.Tools = collection;
            });
        }

        #endregion

        #region Request Settings

        private static async Task GetCurrentLog()
        {
            var log = await Driver.Instance.GetCurrentLog();
            foreach (var message in log)
                await Out.WriteLineAsync(Log.GetStringFormat(message));
        }

        private static async Task GetAllSettings(int handle)
        {
            if (handle != 0)
            {
                await GetAreas(handle);
                await GetSensitivity(handle);
                await GetBindings(handle);
                await GetMiscSettings(handle);
                await GetOutputMode(handle);
                await GetFilters(handle);
                await GetTools();
            }
            else
            {
                foreach (var id in await Driver.Instance.GetActiveTabletHandlerIDs())
                {
                    var tablet = await Driver.Instance.GetTablet(id);
                    var tabletName = tablet.Properties.Name;
                    await Out.WriteLineAsync($"{id}: {tabletName}");
                    await GetAllSettings(id.Value);
                }
            }
        }

        private static async Task GetAreas(int handle)
        {
            var profile = await GetProfile(handle);
            var displayArea = new Area
            {
                Width = profile.DisplayWidth,
                Height = profile.DisplayHeight,
                Position = new Vector2
                {
                    X = profile.DisplayX,
                    Y = profile.DisplayY
                }
            };
            await Out.WriteLineAsync($"Display area: {displayArea}");

            var tabletArea = new Area
            {
                Width = profile.TabletWidth,
                Height = profile.TabletHeight,
                Position = new Vector2
                {
                    X = profile.TabletX,
                    Y = profile.TabletY
                },
                Rotation = profile.TabletRotation
            };
            await Out.WriteLineAsync($"Tablet area: {tabletArea}");
        }

        private static async Task GetSensitivity(int handle)
        {
            var profile = await GetProfile(handle);
            await Out.WriteLineAsync($"Horizontal Sensitivity: {profile.XSensitivity}px/mm");
            await Out.WriteLineAsync($"Vertical Sensitivity: {profile.YSensitivity}px/mm");
            await Out.WriteLineAsync($"Relative mode rotation: {profile.RelativeRotation}°");
            await Out.WriteLineAsync($"Reset time: {profile.ResetTime}");
        }

        private static async Task GetBindings(int handle)
        {
            var profile = await GetProfile(handle);
            await Out.WriteLineAsync($"Tip Binding: {profile.TipButton.Format() ?? "None"}@{profile.TipActivationPressure}%");
            await Out.WriteLineAsync($"Pen Bindings: {string.Join(", ", profile.PenButtons.Format())}");
            await Out.WriteLineAsync($"Express Key Bindings: {string.Join(", ", profile.AuxButtons.Format())}");
        }

        private static async Task GetMiscSettings(int handle)
        {
            var profile = await GetProfile(handle);
            await Out.WriteLineAsync($"Auto hook: {profile.AutoHook}");
            await Out.WriteLineAsync($"Area clipping: {profile.EnableClipping}");
            await Out.WriteLineAsync($"Tablet area limiting: {profile.EnableAreaLimiting}");
            await Out.WriteLineAsync($"Lock aspect ratio: {profile.LockAspectRatio}");
        }

        private static async Task GetOutputMode(int handle)
        {
            var profile = await GetProfile(handle);
            await Out.WriteLineAsync("Output Mode: " + profile.OutputMode.Format());
        }

        private static async Task GetFilters(int handle)
        {
            var profile = await GetProfile(handle);
            await Out.WriteLineAsync("Filters: " + string.Join(", ", profile.Filters.Format()));
        }

        private static async Task GetTools()
        {
            var settings = await GetSettings();
            await Out.WriteLineAsync("Tools: " + string.Join(", ", settings.Tools.Format()));
        }

        #endregion

        #region Actions

        private static async Task Detect()
        {
            await Driver.Instance.DetectTablets();
        }

        #endregion

        #region Debugging

        private static async Task GetString(int handle, int index)
        {
            var str = await Driver.Instance.RequestDeviceString(new TabletHandlerID { Value = handle }, index);
            await Out.WriteLineAsync(str);
        }

        #endregion

        #region List 

        private static async Task ListHandles()
        {
            var ids = await Driver.Instance.GetActiveTabletHandlerIDs();
            foreach (var id in ids)
            {
                var tablet = await Driver.Instance.GetTablet(id);
                var tabletName = tablet.Properties.Name;
                await Out.WriteLineAsync($"{id}: {tabletName}");
            }
        }

        private static async Task ListOutputModes()
        {
            await ListTypes<IOutputMode>();
        }

        private static async Task ListFilters()
        {
            await ListTypes<IPipelineElement<IDeviceReport>>();
        }

        private static async Task ListTools()
        {
            await ListTypes<ITool>();
        }

        private static async Task ListBindings()
        {
            await ListTypes<IBinding>();
        }

        #endregion

        #region Scripting

        private static async Task GetAllSettingsJson()
        {
            var settings = await GetSettings();
            await Out.WriteLineAsync(JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        private static async Task GetDiagnostics()
        {
            var log = await Driver.Instance.GetCurrentLog();
            var diagnostics = new DiagnosticInfo(log);
            await Out.WriteLineAsync(diagnostics.ToString());
        }

        private static async Task STDIO()
        {
            while (await System.Console.In.ReadLineAsync() is string cmd)
                await Root.InvokeAsync(cmd);
        }

        #endregion
    }
}
