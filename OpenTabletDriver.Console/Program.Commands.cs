using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
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
        #region Update

        private static async Task HasUpdate()
        {
            var hasUpdate = await Driver.Instance.CheckForUpdates() is not null;
            await Out.WriteLineAsync(hasUpdate.ToString().ToLowerInvariant());
        }

        private static async Task InstallUpdate()
        {
            if (await Driver.Instance.CheckForUpdates() is not null)
            {
                await Driver.Instance.InstallUpdate();
            }
        }

        #endregion

        #region I/O

        private static async Task LoadSettings(FileInfo file)
        {
            if (Settings.TryDeserialize(file, out var settings))
                await ApplySettings(settings);
            else
                await Out.WriteLineAsync("Invalid settings file");
        }

        private static async Task SaveSettings(FileInfo file)
        {
            var settings = await GetSettings();
            settings.Serialize(file);
        }

        private static async Task ApplyPreset(string name)
        {
            GetAndRefreshPresetDirectory();

            var preset = AppInfo.PresetManager.FindPreset(name);
            await ApplySettings(preset.Settings);
        }

        private static async Task SavePreset(string name)
        {
            var presetDir = GetAndRefreshPresetDirectory();

            var file = new FileInfo(Path.Combine(presetDir.FullName, name + ".json"));

            if (file.Exists)
                throw new ArgumentException("Cannot override presets");

            if (file.Directory == null)
                throw new NullReferenceException(nameof(file.Directory));

            if (file.Directory.FullName != presetDir.FullName)
                throw new ArgumentException("Preset file name must not traverse directories");

            var settings = await GetSettings();

            settings.Serialize(file);
        }

        private static DirectoryInfo GetAndRefreshPresetDirectory()
        {
            var presetDir = new DirectoryInfo(AppInfo.Current.PresetDirectory);

            if (!presetDir.Exists)
                presetDir.Create();

            AppInfo.PresetManager.Refresh();

            return presetDir;
        }

        #endregion

        #region Modify Settings

        private static async Task SetDisplayArea(string tablet, float width, float height, float x, float y)
        {
            await ModifyProfile(tablet, p =>
            {
                p.AbsoluteModeSettings.Display.Width = width;
                p.AbsoluteModeSettings.Display.Height = height;
                p.AbsoluteModeSettings.Display.X = x;
                p.AbsoluteModeSettings.Display.Y = y;
            });
        }

        private static async Task SetTabletArea(string tablet, float width, float height, float x, float y, float rotation = 0)
        {
            await ModifyProfile(tablet, p =>
            {
                p.AbsoluteModeSettings.Tablet.Width = width;
                p.AbsoluteModeSettings.Tablet.Height = height;
                p.AbsoluteModeSettings.Tablet.X = x;
                p.AbsoluteModeSettings.Tablet.Y = y;
                p.AbsoluteModeSettings.Tablet.Rotation = rotation;
            });
        }

        private static async Task SetSensitivity(string tablet, float xSens, float ySens, float rotation = 0)
        {
            await ModifyProfile(tablet, p =>
            {
                p.RelativeModeSettings.XSensitivity = xSens;
                p.RelativeModeSettings.YSensitivity = ySens;
                p.RelativeModeSettings.RelativeRotation = rotation;
            });
        }

        private static async Task SetResetTime(string tablet, int ms)
        {
            await ModifyProfile(tablet, p => p.RelativeModeSettings.ResetTime = TimeSpan.FromMilliseconds(ms));
        }

        private static async Task SetTipBinding(string tablet, string name, float threshold)
        {
            await ModifyProfile(tablet, p =>
            {
                var tipBinding = AppInfo.PluginManager.ConstructObject<IBinding>(name);

                p.BindingSettings.TipButton = new PluginSettingStore(tipBinding);
                p.BindingSettings.TipActivationThreshold = threshold;
            });
        }

        private static async Task SetPenBinding(string tablet, string name, int index)
        {
            await ModifyProfile(tablet, p =>
            {
                var binding = AppInfo.PluginManager.ConstructObject<IBinding>(name);

                p.BindingSettings.PenButtons[index] = new PluginSettingStore(binding);
            });
        }

        private static async Task SetAuxBinding(string tablet, string name, int index)
        {
            await ModifyProfile(tablet, p =>
            {
                var binding = AppInfo.PluginManager.ConstructObject<IBinding>(name);

                p.BindingSettings.AuxButtons[index] = new PluginSettingStore(binding);
            });
        }

        private static async Task SetEnableClipping(string tablet, bool isEnabled)
        {
            await ModifyProfile(tablet, p => p.AbsoluteModeSettings.EnableClipping = isEnabled);
        }

        private static async Task SetEnableAreaLimiting(string tablet, bool isEnabled)
        {
            await ModifyProfile(tablet, p => p.AbsoluteModeSettings.EnableAreaLimiting = isEnabled);
        }

        private static async Task SetLockAspectRatio(string tablet, bool isEnabled)
        {
            await ModifyProfile(tablet, p => p.AbsoluteModeSettings.LockAspectRatio = isEnabled);
        }

        private static async Task SetOutputMode(string tablet, string path)
        {
            var outputMode = PluginSettingStore.FromPath(path);
            if (outputMode == null)
                await Out.WriteLineAsync($"Invalid output mode '{path}'. Use paths like in '{nameof(ListOutputModes).ToLower()}'");
            else
                await ModifyProfile(tablet, p => p.OutputMode = outputMode);
        }

        private static async Task EnableTabletFilters(string tablet, params string[] filters)
        {
            await ModifyProfile(tablet,
                s => AppendPluginStoreSettingsCollectionByPaths<IPositionedPipelineElement<IDeviceReport>>(s.Filters, filters));
        }

        private static async Task EnableTools(params string[] tools)
        {
            await ModifySettings(s => AppendPluginStoreSettingsCollectionByPaths<ITool>(s.Tools, tools));
        }

        private static async Task DisableTabletFilters(string tablet, params string[] filters)
        {
            await ModifyProfile(tablet, s =>
            {
                DisableAllInPluginStoreSettingsCollectionByPaths(s.Filters, filters);
            });
        }

        private static async Task DisableTools(string[] tools)
        {
            await ModifySettings(s =>
            {
                DisableAllInPluginStoreSettingsCollectionByPaths(s.Tools, tools);
            });
        }

        private static async Task ResetTabletFilters(string tablet, params string[] filtersToReset)
        {
            await ModifyProfile(tablet, s =>
            {
                foreach (var filterToReset in filtersToReset)
                    s.Filters = new PluginSettingStoreCollection(s.Filters.Where(filter => filter.Path != filterToReset));
                AppendPluginStoreSettingsCollectionByPaths<IPositionedPipelineElement<IDeviceReport>>(s.Filters, filtersToReset);
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

        private static async Task GetAllSettings()
        {
            var settings = await GetSettings();

            await Out.WriteLineAsync("--- Generic Settings ---");
            await GetTools();

            foreach (var profile in settings.Profiles)
            {
                await Out.WriteLineAsync();
                await Out.WriteLineAsync($"--- Profile for '{profile.Tablet}' ---");
                await GetOutputMode(profile.Tablet);
                await GetAreas(profile.Tablet);
                await GetSensitivity(profile.Tablet);
                await GetBindings(profile.Tablet);
                await GetMiscSettings(profile.Tablet);
                await GetFilters(profile.Tablet);
            }
        }

        private static async Task GetAreas(string tablet)
        {
            var profile = await GetProfile(tablet);
            await Out.WriteLineAsync($"Display area: {profile.AbsoluteModeSettings.Display.Area}");
            await Out.WriteLineAsync($"Tablet area: {profile.AbsoluteModeSettings.Tablet.Area}");
        }

        private static async Task GetSensitivity(string tablet)
        {
            var profile = await GetProfile(tablet);
            await Out.WriteLineAsync($"Horizontal Sensitivity: {profile.RelativeModeSettings.XSensitivity}px/mm");
            await Out.WriteLineAsync($"Vertical Sensitivity: {profile.RelativeModeSettings.YSensitivity}px/mm");
            await Out.WriteLineAsync($"Relative mode rotation: {profile.RelativeModeSettings.RelativeRotation}°");
            await Out.WriteLineAsync($"Reset time: {profile.RelativeModeSettings.ResetTime}");
        }

        private static async Task GetBindings(string tablet)
        {
            var profile = await GetProfile(tablet);
            await Out.WriteLineAsync($"Tip Binding: {profile.BindingSettings.TipButton.Format() ?? "None"}@{profile.BindingSettings.TipActivationThreshold}%");
            await Out.WriteLineAsync($"Pen Bindings: {string.Join(", ", profile.BindingSettings.PenButtons.Format())}");
            await Out.WriteLineAsync($"Express Key Bindings: {string.Join(", ", profile.BindingSettings.AuxButtons.Format())}");
        }

        private static async Task GetMiscSettings(string tablet)
        {
            var profile = await GetProfile(tablet);
            await Out.WriteLineAsync($"Area clipping: {profile.AbsoluteModeSettings.EnableClipping}");
            await Out.WriteLineAsync($"Tablet area limiting: {profile.AbsoluteModeSettings.EnableAreaLimiting}");
            await Out.WriteLineAsync($"Lock aspect ratio: {profile.AbsoluteModeSettings.LockAspectRatio}");
        }

        private static async Task GetOutputMode(string tablet)
        {
            var profile = await GetProfile(tablet);
            await Out.WriteLineAsync("Output Mode: " + profile.OutputMode.Format());
        }

        private static async Task GetFilters(string tablet)
        {
            var profile = await GetProfile(tablet);
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
            await Driver.Instance.SetSettings(await Driver.Instance.GetSettings());
        }

        private static async Task InstallPlugin(string filePath)
        {
            if (!await Driver.Instance.InstallPlugin(filePath))
                await Out.WriteLineAsync("Unable to install plugin");
        }

        private static async Task UninstallPlugin(string folderName)
        {
            var context = AppInfo.PluginManager.GetLoadedPlugins().First(x => x.Directory.Name == folderName);
            await Driver.Instance.UninstallPlugin(context.Directory.FullName);
        }

        #endregion

        #region Debugging

        private static async Task GetString(int vid, int pid, int index)
        {
            var str = await Driver.Instance.RequestDeviceString(vid, pid, index);
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
            // Using the predicate stops output mode types from being listed.
            await ListTypes<IPipelineElement<IDeviceReport>>(t => !t.IsAssignableTo(typeof(IOutputMode)));
        }

        private static async Task ListTools()
        {
            await ListTypes<ITool>();
        }

        private static async Task ListBindings()
        {
            await ListTypes<IBinding>();
        }

        private static async Task ListPresets()
        {
            AppInfo.PresetManager.Refresh();
            foreach (var preset in AppInfo.PresetManager.GetPresets())
                await Out.WriteLineAsync(preset.Name);
        }

        private static async Task ListPlugins()
        {
            foreach (var dir in AppInfo.PluginManager.PluginDirectory.EnumerateDirectories())
                await Out.WriteLineAsync(dir.Name);
        }

        #endregion

        #region Scripting

        private static async Task GetAllSettingsJson()
        {
            var settings = await GetSettings();
            await Out.WriteLineAsync(JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        private static async Task EditSettings()
        {
            string editor = Environment.GetEnvironmentVariable("EDITOR");
            if (!string.IsNullOrWhiteSpace(editor))
            {
                var settings = await GetSettings();
                var tempDir = Environment.GetEnvironmentVariable("TEMP") ?? AppInfo.Current.TemporaryDirectory;
                var tempFile = $"OpenTabletDriver-{Guid.NewGuid()}.json";
                var sha256 = SHA256.Create();

                var path = Path.Join(tempDir, tempFile);
                var cmd = $"{editor} {path}";
                var tokens = cmd.Split(' ');

                var executable = tokens[0];
                var args = string.Join(' ', tokens[1..tokens.Length]);

                if (!Directory.Exists(tempDir))
                    Directory.CreateDirectory(tempDir);

                using (var fs = File.Create(path))
                    Serialization.Serialize(fs, settings);

                var oldHash = GetSHA256(path);

                using (var proc = Process.Start(executable, args))
                    await proc.WaitForExitAsync();

                var newHash = GetSHA256(path);

                using (var fs = File.OpenRead(path))
                    settings = Serialization.Deserialize<Settings>(fs);

                if (oldHash.Equals(newHash))
                {
                    await Out.WriteLineAsync("The file was left unchanged. Settings will not be applied.");
                }
                else
                {
                    await ApplySettings(settings);
                    await Out.WriteLineAsync("Settings were successfully applied.");
                }

                File.Delete(path);
            }
            else
            {
                await Out.WriteLineAsync("The EDITOR environment variable is not set.");
            }
        }

        private static async Task GetDiagnostics()
        {
            try
            {
                var log = await Driver.Instance.GetCurrentLog();
                var diagnostics = new DiagnosticInfo(log, await Driver.Instance.GetDevices());
                await Out.WriteLineAsync(diagnostics.ToString());
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        private static async Task STDIO()
        {
            while (await System.Console.In.ReadLineAsync() is string cmd)
                await Root.InvokeAsync(cmd);
        }

        #endregion
    }
}
