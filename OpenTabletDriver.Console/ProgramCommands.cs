using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using OpenTabletDriver.Console.Attributes;
using OpenTabletDriver.Daemon.Contracts;
using OpenTabletDriver.Daemon.Contracts.Persistence;
using OpenTabletDriver.Output;
using static System.Console;

namespace OpenTabletDriver.Console
{
    public class ProgramCommands : CommandModule
    {
        private readonly IDriverDaemon _driverDaemon;

        public ProgramCommands(IDriverDaemon driverDaemon)
        {
            _driverDaemon = driverDaemon;
        }

        [Command("load", "Load settings from a file")]
        public async Task LoadSettings(FileInfo file)
        {
            if (Settings.TryDeserialize(file, out var settings))
                await ApplySettings(settings);
            else
                await Out.WriteLineAsync("Invalid settings file");
        }

        [Command("save", "Save settings to a file")]
        public async Task SaveSettings(FileInfo file)
        {
            var settings = await GetSettings();
            settings.Serialize(file);
        }

        [Command("apply-preset", "Apply a preset from the presets directory")]
        public async Task ApplyPreset(string name)
        {
            await _driverDaemon.ApplyPreset(name);
        }

        [Command("save-preset", "Save the current settings to the presets directory")]
        public async Task SavePreset(string name)
        {
            var settings = await GetSettings();
            await _driverDaemon.SavePreset(name, settings);
        }

        [Command("set-display-area", "Sets the display area")]
        public async Task SetDisplayArea(string tablet, float width, float height, float x, float y)
        {
            await ModifyProfile(tablet, p =>
            {
                p.OutputMode["Output"].SetValue(new Area
                {
                    Width = width,
                    Height = height,
                    XPosition = x,
                    YPosition = y
                });
            });
        }

        [Command("set-tablet-area", "Sets the tablet area")]
        public async Task SetTabletArea(string tablet, float width, float height, float x, float y, float rotation = 0)
        {
            await ModifyProfile(tablet, p =>
            {
                p.OutputMode["Input"].SetValue(new AngledArea
                {
                    Width = width,
                    Height = height,
                    XPosition = x,
                    YPosition = y,
                    Rotation = rotation
                });
            });
        }

        [Command("set-relative-sensitivity", "Sets the relative sensitivity")]
        public async Task SetSensitivity(string tablet, float xSens, float ySens, float rotation = 0)
        {
            await ModifyProfile(tablet, p =>
            {
                p.OutputMode["XSensitivity"].Value = xSens;
                p.OutputMode["YSensitivity"].Value = ySens;
                p.OutputMode["Rotation"].Value = rotation;
            });
        }

        [Command("set-relative-reset-delay", "Sets the relative mode reset delay")]
        private async Task SetResetDelay(string tablet, int ms)
        {
            await ModifyProfile(tablet, p => p.OutputMode["ResetDelay"].Value = TimeSpan.FromMilliseconds(ms));
        }

        [Command("set-tip-binding", "Sets the tip binding")]
        public async Task SetTipBinding(string tablet, string name, float threshold)
        {
            await ModifyProfile(tablet, async p =>
            {
                var binding = await _driverDaemon.GetDefaults(name);
                p.Bindings.TipButton = binding;
                p.Bindings.TipActivationThreshold = threshold;
            });
        }

        [Command("set-pen-binding", "Sets the pen button bindings")]
        public async Task SetPenBinding(string tablet, string name, int index)
        {
            await ModifyProfile(tablet, async p =>
            {
                var binding = await _driverDaemon.GetDefaults(name);

                p.Bindings.PenButtons[index] = binding;
            });
        }

        [Command("set-aux-binding", "Sets the auxiliary button bindings")]
        public async Task SetAuxBinding(string tablet, string name, int index)
        {
            await ModifyProfile(tablet, async p =>
            {
                var binding = await _driverDaemon.GetDefaults(name);

                p.Bindings.AuxButtons[index] = binding;
            });
        }

        [Command("set-output-mode", "Sets the active output mode with its defaults")]
        public async Task SetOutputMode(string tablet, string mode)
        {
            await ModifyProfile(tablet, async p =>
            {
                var outputMode = await _driverDaemon.GetDefaults(mode);
                p.OutputMode = outputMode;
            });
        }

        [Command("set-filters", "Sets the filters for the active output mode")]
        public async Task SetFilters(string tablet, params string[] filters)
        {
            await ModifyProfile(tablet, async s =>
            {
                var collection = new PluginSettingsCollection();
                foreach (var path in filters)
                {
                    var settings = await _driverDaemon.GetDefaults(path);
                    collection.Add(settings);
                }

                s.Filters = collection;
            });
        }

        [Command("set-tools", "Sets the tools to be enabled")]
        public async Task SetTools(params string[] tools)
        {
            await ModifySettings(async s =>
            {
                var collection = new PluginSettingsCollection();
                foreach (var path in tools)
                {
                    var settings = await _driverDaemon.GetDefaults(path);
                    collection.Add(settings);
                }

                s.Tools = collection;
            });
        }

        [Command("log", "Gets the current log")]
        public async Task GetCurrentLog()
        {
            var log = await _driverDaemon.GetCurrentLog();
            foreach (var message in log)
                await Out.WriteLineAsync(Log.GetStringFormat(message));
        }

        [Command("get-all-settings", "Gets all settings")]
        public async Task GetAllSettings()
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
                await GetFilters(profile.Tablet);
            }
        }

        [Command("get-area-settings", "Gets area settings")]
        public async Task GetAreas(string tablet)
        {
            var settings = await GetSettings();
            var profile = GetProfile(settings, tablet);
            await Out.WriteLineAsync($"Display area: {profile.OutputMode["Output"]}");
            await Out.WriteLineAsync($"Tablet area: {profile.OutputMode["Input"]}");
        }

        [Command("get-sensitivity", "Gets relative mode sensitivity settings")]
        public async Task GetSensitivity(string tablet)
        {
            var settings = await GetSettings();
            var profile = GetProfile(settings, tablet);
            await Out.WriteLineAsync($"Sensitivity: {profile.OutputMode["Sensitivity"]}px/mm");
            await Out.WriteLineAsync($"Rotation: {profile.OutputMode["Rotation"]}°");
            await Out.WriteLineAsync($"Reset time: {profile.OutputMode["ResetDelay"]}");
        }

        [Command("get-bindings", "Gets binding settings")]
        public async Task GetBindings(string tablet)
        {
            var settings = await GetSettings();
            var profile = GetProfile(settings, tablet);
            await Out.WriteLineAsync($"Tip Binding: {profile.Bindings.TipButton.Format()}@{profile.Bindings.TipActivationThreshold}%");
            await Out.WriteLineAsync($"Pen Bindings: {profile.Bindings.PenButtons.Format()}");
            await Out.WriteLineAsync($"Express Key Bindings: {profile.Bindings.AuxButtons.Format()}");
        }

        [Command("get-output-mode", "Gets the output mode")]
        public async Task GetOutputMode(string tablet)
        {
            var settings = await GetSettings();
            var profile = GetProfile(settings, tablet);
            await Out.WriteLineAsync("Output Mode: " + profile.OutputMode.Format());
        }

        [Command("get-filters", "Gets all filters applied to the output mode")]
        public async Task GetFilters(string tablet)
        {
            var settings = await GetSettings();
            var profile = GetProfile(settings, tablet);
            await Out.WriteLineAsync("Filters: " + profile.Filters.Format());
        }

        [Command("get-tools", "Gets all tools")]
        public async Task GetTools()
        {
            var settings = await GetSettings();
            await Out.WriteLineAsync("Tools: " + settings.Tools.Format());
        }

        [Command("get-string", "Requests a device string")]
        public async Task GetString(int vid, int pid, int index)
        {
            var str = await _driverDaemon.RequestDeviceString(vid, pid, index);
            await Out.WriteLineAsync(str);
        }

        [Command("detect", "Scan all devices for supported tablets")]
        public async Task Detect()
        {
            await _driverDaemon.DetectTablets();
        }

        [Command("list-presets", "Lists all saved presets")]
        public async Task ListPresets()
        {
            var presets = await _driverDaemon.GetPresets();
            var output = string.Join(", ", presets);
            await Out.WriteLineAsync(output);
        }

        [Command("list-tablets", "Lists all connected tablets")]
        public async Task ListTablets()
        {
            var tablets = new List<(int, string, InputDeviceState)>();
            foreach (var tablet in await _driverDaemon.GetTablets())
            {
                var id = tablet;
                var name = (await _driverDaemon.GetTabletConfiguration(id)).Name;
                var state = await _driverDaemon.GetTabletState(id);
                tablets.Add((id, name, state));
            }

            var stringBuilder = new StringBuilder();
            foreach (var (id, name, state) in tablets)
                stringBuilder.AppendLine($"{id}: {name} ({state})");
            await Out.WriteLineAsync(stringBuilder.ToString());
        }

        [Command("list-output-modes", "Lists all output modes")]
        public async Task ListOutputModes() => await ListTypes<IOutputMode>();

        [Command("list-filters", "Lists all supported filters")]
        public async Task ListFilters() => await ListTypes<IDevicePipelineElement>();

        [Command("list-bindings", "Lists all supported bindings")]
        public async Task ListBindings() => await ListTypes<IBinding>();

        [Command("list-tools", "Lists all supported tools")]
        public async Task ListTools() => await ListTypes<ITool>();

        private async Task ListTypes<T>()
        {
            foreach (var type in await _driverDaemon.GetMatchingTypes(typeof(T).GetPath()))
            {
                var output = type.FriendlyName != null ? $"{type.Path} [{type.FriendlyName}]" : type.Path;
                await Out.WriteLineAsync(output);
            }
        }

        [Command("edit-settings", "Edit settings with your editor")]
        public async Task EditSettings(string? editor = null)
        {
            editor ??= Environment.GetEnvironmentVariable("EDITOR");

            if (string.IsNullOrWhiteSpace(editor))
            {
                await Out.WriteLineAsync("No editor is defined in the EDITOR environment variable.");
                return;
            }

            var settings = await GetSettings();
            var appInfo = await _driverDaemon.GetApplicationInfo();

            var tempDir = Environment.GetEnvironmentVariable("TEMP") ?? appInfo.TemporaryDirectory;
            var tempFile = $"OpenTabletDriver-{Guid.NewGuid()}.json";
            using var sha256 = SHA256.Create();

            var path = Path.Join(tempDir, tempFile);
            var cmd = $"{editor} {path}";
            var tokens = cmd.Split(' ');

            var executable = tokens[0];
            var args = string.Join(' ', tokens[1..tokens.Length]);

            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            await using (var fs = File.Create(path))
                Serialization.Serialize(fs, settings);

            var oldHash = sha256.ComputeFileHash(path);

            using (var proc = Process.Start(executable, args))
                await proc.WaitForExitAsync();

            var newHash = sha256.ComputeFileHash(path);

            if (oldHash.Equals(newHash))
            {
                await Out.WriteLineAsync("The file was left unchanged. Settings will not be applied.");
            }
            else
            {
                await using (var fs = File.OpenRead(path))
                {
                    var newSettings = Serialization.Deserialize<Settings>(fs)!;
                    await ApplySettings(newSettings);
                    await Out.WriteLineAsync("Settings were successfully applied.");
                }
            }

            if (File.Exists(path))
                File.Delete(path);
        }

        [Command("diagnostics", "Gets OpenTabletDriver diagnostics")]
        public async Task GetDiagnostics()
        {
            try
            {
                var diagnostics = await _driverDaemon.GetDiagnostics();
                await Out.WriteLineAsync(diagnostics.ToString());
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }

        [Command("update", "Install OpenTabletDriver update if available")]
        public async Task InstallUpdate()
        {
            if (await _driverDaemon.CheckForUpdates() is not null)
            {
                await _driverDaemon.InstallUpdate();
            }
        }

        private async Task<Settings> GetSettings()
        {
            return await _driverDaemon.GetSettings();
        }

        private Profile GetProfile(Settings settings, string tablet)
        {
            return settings.Profiles.First(p => p.Tablet.Equals(tablet, StringComparison.InvariantCultureIgnoreCase));
        }

        private async Task ApplySettings(Settings settings)
        {
            await _driverDaemon.ApplySettings(settings);
        }

        private async Task ModifySettings(Action<Settings> action)
        {
            var settings = await GetSettings();
            action.Invoke(settings);
            await ApplySettings(settings);
        }

        private async Task ModifySettings(Func<Settings, Task> func)
        {
            var settings = await GetSettings();
            await func.Invoke(settings);
            await ApplySettings(settings);
        }

        private async Task ModifyProfile(string tablet, Action<Profile> action)
        {
            await ModifySettings(s =>
            {
                var profile = GetProfile(s, tablet);
                action.Invoke(profile);
            });
        }

        private async Task ModifyProfile(string tablet, Func<Profile, Task> func)
        {
            await ModifySettings(async s =>
            {
                var profile = GetProfile(s, tablet);
                await func.Invoke(profile);
            });
        }
    }
}
