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
            var settings = Settings.Deserialize(file);
            await ApplySettings(settings);
        }

        private static async Task SaveSettings(FileInfo file)
        {
            var settings = await GetSettings();
            settings.Serialize(file);
        }

        #endregion

        #region Modify Settings

        private static async Task SetDisplayArea(float width, float height, float x, float y)
        {
            await ModifySettings(s =>
            {
                s.DisplayWidth = width;
                s.DisplayHeight = height;
                s.DisplayX = x;
                s.DisplayY = y;
            });
        }

        private static async Task SetTabletArea(float width, float height, float x, float y, float rotation = 0)
        {
            await ModifySettings(s =>
            {
                s.TabletWidth = width;
                s.TabletHeight = height;
                s.TabletX = x;
                s.TabletY = y;
                s.TabletRotation = rotation;
            });
        }

        private static async Task SetSensitivity(float xSens, float ySens, float rotation = 0)
        {
            await ModifySettings(s =>
            {
                s.XSensitivity = xSens;
                s.YSensitivity = ySens;
                s.RelativeRotation = rotation;
            });
        }

        private static async Task SetResetTime(int ms)
        {
            await ModifySettings(s => s.ResetTime = TimeSpan.FromMilliseconds(ms));
        }

        private static async Task SetTipBinding(string name, string property, float threshold)
        {
            await ModifySettings(s =>
            {
                var tipBinding = AppInfo.PluginManager.ConstructObject<IBinding>(name);
                tipBinding.Property = property;

                s.TipButton = new PluginSettingStore(tipBinding);
                s.TipActivationPressure = threshold;
            });
        }

        private static async Task SetPenBinding(string name, string property, int index)
        {
            await ModifySettings(s =>
            {
                var binding = AppInfo.PluginManager.ConstructObject<IBinding>(name);
                binding.Property = property;

                s.PenButtons[index] = new PluginSettingStore(binding);
            });
        }

        private static async Task SetAuxBinding(string name, string property, int index)
        {
            await ModifySettings(s =>
            {
                var binding = AppInfo.PluginManager.ConstructObject<IBinding>(name);
                binding.Property = property;

                s.AuxButtons[index] = new PluginSettingStore(binding);
            });
        }

        private static async Task SetAutoHook(bool isEnabled)
        {
            await ModifySettings(s => s.AutoHook = isEnabled);
        }

        private static async Task SetEnableClipping(bool isEnabled)
        {
            await ModifySettings(s => s.EnableClipping = isEnabled);
        }

        private static async Task SetEnableAreaLimiting(bool isEnabled)
        {
            await ModifySettings(s => s.EnableAreaLimiting = isEnabled);
        }

        private static async Task SetLockAspectRatio(bool isEnabled)
        {
            await ModifySettings(s => s.LockAspectRatio = isEnabled);
        }

        private static async Task SetOutputMode(string mode)
        {
            await ModifySettings(s => s.OutputMode = new PluginSettingStore(mode));
        }

        private static async Task SetFilters(IEnumerable<string> filters)
        {
            await ModifySettings(s =>
            {
                var collection = new PluginSettingStoreCollection();
                foreach (var path in filters)
                    collection.Add(new PluginSettingStore(path));

                s.Filters = collection;
            });
        }

        private static async Task SetTools(IEnumerable<string> tools)
        {
            await ModifySettings(s =>
            {
                var collection = new PluginSettingStoreCollection();
                foreach (var path in tools)
                    collection.Add(new PluginSettingStore(path));

                s.Tools = collection;
            });
        }

        private static async Task SetInputHook(bool isHooked)
        {
            await Driver.Instance.EnableInput(isHooked);
        }

        #endregion

        #region Request Settings

        private static async Task GetCurrentLog()
        {
            var log = await Driver.Instance.GetCurrentLog();
            foreach (var message in log)
                await Out.WriteLineAsync(Log.GetStringFormat(message));
        }

        private static async Task GetAllSettings()
        {
            await GetAreas();
            await GetSensitivity();
            await GetBindings();
            await GetMiscSettings();
            await GetOutputMode();
            await GetFilters();
            await GetTools();
        }

        private static async Task GetAreas()
        {
            var settings = await GetSettings();
            var displayArea = new Area
            {
                Width = settings.DisplayWidth,
                Height = settings.DisplayHeight,
                Position = new Vector2
                {
                    X = settings.DisplayX,
                    Y = settings.DisplayY
                }
            };
            await Out.WriteLineAsync($"Display area: {displayArea}");

            var tabletArea = new Area
            {
                Width = settings.TabletWidth,
                Height = settings.TabletHeight,
                Position = new Vector2
                {
                    X = settings.TabletX,
                    Y = settings.TabletY
                },
                Rotation = settings.TabletRotation
            };
            await Out.WriteLineAsync($"Tablet area: {tabletArea}");
        }

        private static async Task GetSensitivity()
        {
            var settings = await GetSettings();
            await Out.WriteLineAsync($"Horizontal Sensitivity: {settings.XSensitivity}px/mm");
            await Out.WriteLineAsync($"Vertical Sensitivity: {settings.YSensitivity}px/mm");
            await Out.WriteLineAsync($"Relative mode rotation: {settings.RelativeRotation}°");
            await Out.WriteLineAsync($"Reset time: {settings.ResetTime}");
        }

        private static async Task GetBindings()
        {
            var settings = await GetSettings();
            await Out.WriteLineAsync($"Tip Binding: {settings.TipButton.Format() ?? "None"}@{settings.TipActivationPressure}%");
            await Out.WriteLineAsync($"Pen Bindings: {string.Join(", ", settings.PenButtons.Format())}");
            await Out.WriteLineAsync($"Express Key Bindings: {string.Join(", ", settings.AuxButtons.Format())}");
        }

        private static async Task GetMiscSettings()
        {
            var settings = await GetSettings();
            await Out.WriteLineAsync($"Auto hook: {settings.AutoHook}");
            await Out.WriteLineAsync($"Area clipping: {settings.EnableClipping}");
            await Out.WriteLineAsync($"Tablet area limiting: {settings.EnableAreaLimiting}");
            await Out.WriteLineAsync($"Lock aspect ratio: {settings.LockAspectRatio}");
        }

        private static async Task GetOutputMode()
        {
            var settings = await GetSettings();
            await Out.WriteLineAsync("Output Mode: " + settings.OutputMode.Format());
        }

        private static async Task GetFilters()
        {
            var settings = await GetSettings();
            await Out.WriteLineAsync("Filters: " + string.Join(", ", settings.Filters.Format()));
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

        private static async Task GetString(int index)
        {
            var str = await Driver.Instance.RequestDeviceString(index);
            await Out.WriteLineAsync(str);
        }

        #endregion

        #region List Types

        private static async Task ListOutputModes()
        {
            await ListTypes<IOutputMode>();
        }

        private static async Task ListFilters()
        {
            await ListTypes<IFilter>();
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
