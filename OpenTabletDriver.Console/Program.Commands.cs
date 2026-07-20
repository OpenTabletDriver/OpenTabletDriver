using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Tablet;
using static System.Console;

namespace OpenTabletDriver.Console
{
    partial class Program
    {
        #region Update

        private static async Task HasUpdate()
        {
            if (!await EnsureDaemonReady()) return;
            var hasUpdate = await Driver.Instance!.CheckForUpdates() is not null;
            await Out.WriteLineAsync(hasUpdate.ToString().ToLowerInvariant());
        }

        private static async Task InstallUpdate()
        {
            if (!await EnsureDaemonReady()) return;
            if (await Driver.Instance!.CheckForUpdates() is not null)
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

        private static async Task SaveDefaultSettings()
        {
            await SaveSettings(new FileInfo(AppInfo.Current.SettingsFile));
        }

        private static async Task ApplyPreset(string name)
        {
            if (!await EnsureDaemonReady()) return;
            GetAndRefreshPresetDirectory();

            var preset = AppInfo.PresetManager.FindPreset(name);
            if (preset == null) throw new ArgumentException($"Preset {name} not found");
            await ApplySettings(preset.Settings);
        }

        private static async Task SavePreset(string name)
        {
            if (!await EnsureDaemonReady()) return;
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
                if (p.AbsoluteModeSettings == null)
                    throw new InvalidOperationException($"Could not find {nameof(p.AbsoluteModeSettings)} in profile");

                p.AbsoluteModeSettings.Display.Width = width;
                p.AbsoluteModeSettings.Display.Height = height;
                p.AbsoluteModeSettings.Display.X = x;
                p.AbsoluteModeSettings.Display.Y = y;
            });
        }

        private static async Task MapToDisplayIndex(string tablet, uint index)
        {
            if (DesktopInterop.VirtualScreen == null)
                throw new InvalidOperationException($"Could not look up {nameof(DesktopInterop.VirtualScreen)}");

            var displays = DesktopInterop.VirtualScreen.Displays.ToArray();

            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, (uint)displays.Length);

            var display = displays[index];

            float width = display.Width;
            float height = display.Height;

            // account for monitor layouts with negative offsets (e.g. Wayland supports this)
            // skip IVirtualScreen's as these tend to be normalized to 0,0, which may confuse these methods
            float xOffset = displays.Where(d => d is not IVirtualScreen).MinBy(d => d.Position.X)?.Position.X ?? 0;
            float yOffset = displays.Where(d => d is not IVirtualScreen).MinBy(d => d.Position.Y)?.Position.Y ?? 0;

            float x, y;

            if (display is IVirtualScreen virtualScreen)
            {
                x = virtualScreen.Width / 2;
                y = virtualScreen.Height / 2;
            }
            else
            {
                virtualScreen = DesktopInterop.VirtualScreen;
                x = display.Position.X - xOffset + virtualScreen.Position.X + (width / 2);
                y = display.Position.Y - yOffset + virtualScreen.Position.Y + (height / 2);
            }

            await SetDisplayArea(tablet, width, height, x, y);
        }

        private static async Task SetTabletArea(string tablet, float width, float height, float x, float y, float rotation = 0)
        {
            await ModifyProfile(tablet, p =>
            {
                if (p.AbsoluteModeSettings == null)
                    throw new InvalidOperationException($"Could not find {nameof(p.AbsoluteModeSettings)} in profile");

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
                if (p.RelativeModeSettings == null)
                    throw new InvalidOperationException($"Could not find {nameof(p.RelativeModeSettings)} in profile");

                p.RelativeModeSettings.XSensitivity = xSens;
                p.RelativeModeSettings.YSensitivity = ySens;
                p.RelativeModeSettings.RelativeRotation = rotation;
            });
        }

        private static async Task SetResetTime(string tablet, int ms)
        {
            await ModifyProfile(tablet, p =>
            {
                if (p.RelativeModeSettings == null)
                    throw new InvalidOperationException($"Could not find {nameof(p.RelativeModeSettings)} in profile");

                p.RelativeModeSettings.ResetTime = TimeSpan.FromMilliseconds(ms);
            });
        }

        private static async Task SetTipBinding(string tablet, string name, float threshold)
        {
            await ModifyProfile(tablet, p =>
            {
                var tipBinding = AppInfo.PluginManager.ConstructObject<IBinding>(name) ?? throw new InvalidOperationException($"Could not construct binding with name {name}");

                p.BindingSettings.TipButton = new PluginSettingStore(tipBinding);
                p.BindingSettings.TipActivationThreshold = threshold;
            });
        }

        private static async Task SetPenBinding(string tablet, string name, int index)
        {
            await ModifyProfile(tablet, p =>
            {
                var binding = AppInfo.PluginManager.ConstructObject<IBinding>(name) ?? throw new InvalidOperationException($"Could not construct binding with name {name}");

                p.BindingSettings.PenButtons[index] = new PluginSettingStore(binding);
            });
        }

        private static async Task SetAuxBinding(string tablet, string name, int index)
        {
            await ModifyProfile(tablet, p =>
            {
                var binding = AppInfo.PluginManager.ConstructObject<IBinding>(name) ?? throw new InvalidOperationException($"Could not construct binding with name {name}");

                p.BindingSettings.AuxButtons[index] = new PluginSettingStore(binding);
            });
        }

        private static async Task SetEnableClipping(string tablet, bool isEnabled)
        {
            await ModifyProfile(tablet, p =>
            {
                if (p.AbsoluteModeSettings == null)
                    throw new InvalidOperationException($"Could not find {nameof(p.AbsoluteModeSettings)} in profile");

                p.AbsoluteModeSettings.EnableClipping = isEnabled;
            });
        }

        private static async Task SetEnableAreaLimiting(string tablet, bool isEnabled)
        {
            await ModifyProfile(tablet, p =>
            {
                if (p.AbsoluteModeSettings == null)
                    throw new InvalidOperationException($"Could not find {nameof(p.AbsoluteModeSettings)} in profile");

                p.AbsoluteModeSettings.EnableAreaLimiting = isEnabled;
            });
        }

        private static async Task SetLockAspectRatio(string tablet, bool isEnabled)
        {
            await ModifyProfile(tablet, p =>
            {
                if (p.AbsoluteModeSettings == null)
                    throw new InvalidOperationException($"Could not find {nameof(p.AbsoluteModeSettings)} in profile");

                p.AbsoluteModeSettings.LockAspectRatio = isEnabled;
            });
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
                    s.Filters = new PluginSettingStoreCollection(s.Filters.Where(filter => filter?.Path != filterToReset)!);
                AppendPluginStoreSettingsCollectionByPaths<IPositionedPipelineElement<IDeviceReport>>(s.Filters, filtersToReset);
            });
        }

        #endregion

        #region Request Settings

        private static async Task GetCurrentLog()
        {
            if (!await EnsureDaemonReady()) return;
            var log = await Driver.Instance!.GetCurrentLog();
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

            if (profile.AbsoluteModeSettings == null)
                throw new InvalidOperationException($"Could not find {nameof(profile.AbsoluteModeSettings)} in profile");

            await Out.WriteLineAsync($"Display area: {profile.AbsoluteModeSettings.Display.Area}");
            await Out.WriteLineAsync($"Tablet area: {profile.AbsoluteModeSettings.Tablet.Area}");
        }

        private static async Task GetSensitivity(string tablet)
        {
            var profile = await GetProfile(tablet);

            if (profile.RelativeModeSettings == null)
                throw new InvalidOperationException($"Could not find {nameof(profile.RelativeModeSettings)} in profile");

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
            await Out.WriteLineAsync($"Wheel Bindings:\n{string.Join("\n", profile.BindingSettings.WheelBindings.Format())}");
        }

        private static async Task GetMiscSettings(string tablet)
        {
            var profile = await GetProfile(tablet);

            if (profile.AbsoluteModeSettings == null)
                throw new InvalidOperationException($"Could not find {nameof(profile.AbsoluteModeSettings)} in profile");

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
            if (!await EnsureDaemonReady()) return;
            await Driver.Instance!.DetectTablets();
            await Driver.Instance!.SetSettings(await Driver.Instance.GetSettings());
        }

        private static async Task InstallPlugin(string filePath)
        {
            if (!await EnsureDaemonReady()) return;
            if (!await Driver.Instance!.InstallPlugin(filePath))
                await Out.WriteLineAsync("Unable to install plugin");
        }

        private static async Task UninstallPlugin(string folderName)
        {
            if (!await EnsureDaemonReady()) return;
            var context = AppInfo.PluginManager.GetLoadedPlugins().First(x => x.Directory.Name == folderName);
            await Driver.Instance!.UninstallPlugin(context.Directory.FullName);
        }

        #endregion

        #region Debugging

        private static async Task GetString(int vid, int pid, int index)
        {
            if (!await EnsureDaemonReady()) return;
            var str = await Driver.Instance!.RequestDeviceString(vid, pid, index);
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
            if (!await EnsureDaemonReady()) return;
            foreach (var dir in AppInfo.PluginManager.PluginDirectory.EnumerateDirectories())
                await Out.WriteLineAsync(dir.Name);
        }

        // BUG: DesktopInterop takes the CLI's view of the display layout - this may be desynched
        private static async Task ListDisplays()
        {
            if (DesktopInterop.VirtualScreen == null)
                throw new InvalidOperationException($"Unable to look up {nameof(DesktopInterop.VirtualScreen)}");

            int index = 0;
            foreach (var display in DesktopInterop.VirtualScreen.Displays)
                await Out.WriteLineAsync($"{index++}: {display}");
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
            string? editor = Environment.GetEnvironmentVariable("EDITOR");
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

                await using (var fs = File.Create(path))
                    Serialization.Serialize(fs, settings);

                var oldHash = GetSHA256(path);

                using (var proc = Process.Start(executable, args))
                    await proc.WaitForExitAsync();

                var newHash = GetSHA256(path);

                await using (var fs = File.OpenRead(path))
                    settings = Serialization.Deserialize<Settings>(fs)
                        ?? throw new InvalidOperationException($"Unable to deserialize settings: {path}");

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
            if (!await EnsureDaemonReady()) return;
            try
            {
                var diagnostics = await Driver.Instance!.GetDiagnosticInfo();
                await Out.WriteLineAsync(diagnostics.ToString());
            }
            catch (Exception ex)
            {
                await Out.WriteLineAsync(ex.ToString());
            }
        }

        private static async Task STDIO()
        {
            while (await System.Console.In.ReadLineAsync() is string cmd)
                await Root.Parse(cmd).InvokeAsync();
        }

        #endregion
    }
}
