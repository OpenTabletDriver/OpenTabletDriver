using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Output;

#nullable enable

namespace OpenTabletDriver.Desktop.Migration
{
    using V6 = LegacySettings.V6;

    public class SettingsMigrator
    {
        private readonly IServiceProvider _serviceProvider;

        public SettingsMigrator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Migrate(IAppInfo appInfo)
        {
            var file = new FileInfo(appInfo.SettingsFile);

            if (Migrate(file) is Settings settings)
            {
                Log.Write("Settings", "Settings have been migrated.");

                // Back up existing settings file for safety
                var backupDir = appInfo.BackupDirectory;
                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);

                string timestamp = DateTime.UtcNow.ToString(".yyyy-MM-dd_hh-mm-ss");
                var backupPath = Path.Join(backupDir, file.Name + timestamp + ".old");
                file.CopyTo(backupPath, true);

                Serialization.Serialize(file, settings);
            }
        }

        private Settings? Migrate(FileInfo file)
        {
            var v6 = Serialization.Deserialize<V6::Settings>(file);
            return v6.IsValid() ? Convert(v6) : null;
        }

        private Settings? Convert(V6::Settings oldSettings)
        {
            var settings = new Settings
            {
                Tools = new PluginSettingsCollection(oldSettings.Tools)
            };

            foreach (var oldProfile in oldSettings.Profiles)
            {
                var newProfile = new Profile
                {
                    Tablet = oldProfile.Tablet,
                    Filters = oldProfile.Filters,
                    BindingSettings = oldProfile.BindingSettings,
                    OutputMode = MigrateOutputModeSettings(oldSettings, oldProfile)
                };
                settings.Profiles.Add(newProfile);
            }

            if (settings.Profiles.Any() && settings.Profiles.All(p => !string.IsNullOrWhiteSpace(p.Tablet)))
                return settings;

            return null;
        }

        private PluginSettings? MigrateOutputModeSettings(V6::Settings oldSettings, V6::Profile oldProfile)
        {
            var pluginFactory =  _serviceProvider.GetRequiredService<IPluginFactory>();

            var oldSetting = oldProfile.OutputMode;
            var type = pluginFactory.GetPluginType(oldSetting.Path);
            if (type == null)
                return null;

            bool isAbsolute = type.IsAssignableTo(typeof(AbsoluteOutputMode));
            bool isRelative = type.IsAssignableTo(typeof(RelativeOutputMode));

            if (isAbsolute)
            {
                var absSettings = oldProfile.AbsoluteModeSettings;
                return new PluginSettings(type, new
                {
                    Input = new AngledArea
                    {
                        Width = absSettings.Tablet.Width,
                        Height = absSettings.Tablet.Height,
                        XPosition = absSettings.Tablet.X,
                        YPosition = absSettings.Tablet.Y,
                        Rotation = absSettings.Tablet.Rotation
                    },
                    Output = new Area
                    {
                        Width = absSettings.Display.Width,
                        Height = absSettings.Display.Height,
                        XPosition = absSettings.Display.X,
                        YPosition = absSettings.Display.Y
                    },
                    LockAspectRatio = absSettings.LockAspectRatio,
                    AreaClipping = absSettings.EnableClipping,
                    AreaLimiting = absSettings.EnableAreaLimiting,
                    AreaBounds = oldSettings.LockUsableAreaDisplay || oldSettings.LockUsableAreaTablet
                });
            }

            if (isRelative)
            {
                var relSettings = oldProfile.RelativeModeSettings;
                return new PluginSettings(type, new
                {
                    Sensitivity = new Vector2(relSettings.XSensitivity, relSettings.YSensitivity),
                    Rotation = relSettings.RelativeRotation,
                    ResetDelay = relSettings.ResetTime
                });
            }

            return null;
        }

        private static readonly Dictionary<Regex, string> NamespaceMigrationDict = new Dictionary<Regex, string>
        {
            { new Regex(@"TabletDriverLib\.(.+?)$"), $"OpenTabletDriver.{{0}}" },
            { new Regex(@"OpenTabletDriver\.Binding\.(.+?)$"), $"OpenTabletDriver.Desktop.Binding.{{0}}" },
            { new Regex(@"OpenTabletDriver\.Output\.(.+?)$"), $"OpenTabletDriver.Desktop.Output.{{0}}" }
        };

        private static readonly Dictionary<Regex, (string, string)> PropertyMigrationDict = new Dictionary<Regex, (string, string)>
        {
            { new Regex(@"OpenTabletDriver\.Desktop\.Binding\.KeyBinding$"), ("^Property$", "Key") },
            { new Regex(@"OpenTabletDriver\.Desktop\.Binding\.MouseBinding$"), ("^Property$", "Button") }
        };

        private PluginSettings? SafeMigrate(PluginSettings? store, PluginSettings? defaultStore = null)
        {
            store = SafeMigrateNamespace(store, defaultStore);
            store = MigrateProperty(store);
            return store;
        }

        private static void MigrateNamespace(PluginSettings? store)
        {
            if (store != null)
            {
                store.Path = MigrateNamespace(store.Path) ?? string.Empty;
            }
        }

        private static string? MigrateNamespace(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            foreach (var pair in NamespaceMigrationDict)
            {
                var regex = pair.Key;
                var replacement = pair.Value;

                var match = regex.Match(input);
                if (match.Success)
                    input = string.Format(replacement, match.Groups[1]);
            }

            return input;
        }

        private static PluginSettings? MigrateProperty(PluginSettings? store)
        {
            if (store != null)
            {
                foreach (var pair in PropertyMigrationDict)
                {
                    var type = pair.Key;
                    var (property, replacementProperty) = pair.Value;

                    if (type.IsMatch(store.Path ?? string.Empty))
                    {
                        foreach (var setting in store.Settings!)
                        {
                            if (Regex.IsMatch(setting.Property, property))
                            {
                                setting.Property = replacementProperty;
                            }
                        }
                    }
                }
            }

            return store;
        }

        private PluginSettings? SafeMigrateNamespace(PluginSettings? store, PluginSettings? defaultStore)
        {
            MigrateNamespace(store);
            if (store != null && StoreFromTypePath(store.Path) == null && defaultStore != null)
            {
                Log.Write("Settings", $"Invalid plugin path '{store.Path}' has been changed to '{defaultStore.Path}'", LogLevel.Warning);
                store = defaultStore;
            }
            return store;
        }

        private PluginSettings? StoreFromTypePath(string? path)
        {
            var pluginFactory = _serviceProvider.GetRequiredService<IPluginFactory>();
            var type = pluginFactory.GetPluginType(path ?? string.Empty);
            if (type == null)
                return null;

            return new PluginSettings(type);
        }

        private PluginSettingsCollection SafeMigrateCollection(PluginSettingsCollection? collection)
        {
            if (collection == null)
                collection = new PluginSettingsCollection();

            for (int i = 0; i < collection.Count; i++)
                collection[i] = SafeMigrate(collection[i]);

            return collection;
        }
    }
}
