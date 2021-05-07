using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Binding;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Migration;
using OpenTabletDriver.Desktop.Output;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop
{
    public static class ProfileSerializer
    {
        static ProfileSerializer()
        {
            serializer.Error += SerializationErrorHandler;
        }

        private static readonly Regex propertyValueRegex = new Regex(PROPERTY_VALUE_REGEX, RegexOptions.Compiled);
        private const string PROPERTY_VALUE_REGEX = "\\\"(.+?)\\\"";
        private static readonly JsonSerializer serializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };

        public static IEnumerable<Profile> GetCompatibleProfiles(TabletHandlerID id)
        {
            var profileDir = PrepareProfileDirectory(GetTabletName(id));
            foreach (var profileFile in profileDir.EnumerateFiles().Where(f => f.Extension == ".json"))
            {
                yield return Deserialize(profileFile);
            }
        }

        public static Profile GetDefaultProfile(TabletHandlerID id, string profileName = "Default")
        {
            var virtualScreen = DesktopInterop.VirtualScreen;
            var properties = Info.Driver.GetTabletState(id).Properties;
            var tabletName = properties.Name;
            var tablet = properties.Specifications.Digitizer;

            return new Profile
            {
                ProfileName = profileName,
                CompatibleDevice = tabletName,
                OutputMode = new PluginSettingStore(typeof(AbsoluteMode)),
                DisplayWidth = virtualScreen.Width,
                DisplayHeight = virtualScreen.Height,
                DisplayX = virtualScreen.Width / 2,
                DisplayY = virtualScreen.Height / 2,
                TabletWidth = tablet?.Width ?? 0,
                TabletHeight = tablet?.Height ?? 0,
                TabletX = tablet?.Width / 2 ?? 0,
                TabletY = tablet?.Height / 2 ?? 0,
                AutoHook = true,
                EnableClipping = true,
                LockUsableAreaDisplay = true,
                LockUsableAreaTablet = true,
                TipButton = new PluginSettingStore(
                    new MouseBinding
                    {
                        Button = nameof(Plugin.Platform.Pointer.MouseButton.Left)
                    }
                ),
                TipActivationPressure = 1,
                PenButtons = new PluginSettingStoreCollection(),
                AuxButtons = new PluginSettingStoreCollection(),
                XSensitivity = 10,
                YSensitivity = 10,
                RelativeRotation = 0,
                ResetTime = TimeSpan.FromMilliseconds(100)
            };
        }

        public static DirectoryInfo PrepareProfileDirectory(string tabletName)
        {
            if (!Directory.Exists(AppInfo.Current.ProfileDirectory))
            {
                Directory.CreateDirectory(AppInfo.Current.ProfileDirectory);
                Log.Write("Settings", $"Created OpenTabletDriver profiles directory: {AppInfo.Current.ProfileDirectory}");
            }

            var tabletDir = new DirectoryInfo(Path.Join(AppInfo.Current.ProfileDirectory, tabletName));
            if (!tabletDir.Exists)
            {
                tabletDir.Create();
                Log.Write("Settings", $"Created tablet profile directory: {tabletDir.FullName}");
            }

            return tabletDir;
        }

        public static void Serialize(Profile profile)
        {
            var profileDir = PrepareProfileDirectory(profile.CompatibleDevice);
            var profileFile = new FileInfo(Path.Join(profileDir.FullName, profile.ProfileName + ".json"));

            Serialize(profile, profileFile);
        }

        public static void Serialize(Profile profile, FileInfo profileFile)
        {
            if (profileFile.Exists)
                profileFile.Delete();

            using (var sw = profileFile.CreateText())
            using (var jw = new JsonTextWriter(sw))
                serializer.Serialize(jw, profile);
        }

        public static Profile Deserialize(TabletHandlerID id, string profileName)
        {
            var tabletName = GetTabletName(id);
            var profileDir = PrepareProfileDirectory(tabletName);
            var profileFile = new FileInfo(Path.Join(profileDir.FullName, profileName + ".json"));

            Profile profile;

            try
            {
                profile = Deserialize(profileFile);
            }
            catch
            {
                Log.Write("Settings", "Profile deserialization failed. Attempting recovery.", LogLevel.Error);
                profile = GetDefaultProfile(id, profileName);
                Recover(profileFile, profile);

                Log.Write("Settings", "Recovery complete");
                return profile;
            }

            if (profile != null)
            {
                return profile;
            }
            else
            {
                Log.Write("Settings", $"Generated profile {profileName} using defaults");
                return GetDefaultProfile(id, profileName);
            }
        }

        public static Profile Deserialize(FileInfo file)
        {
            if (file.Exists)
            {
                using (var stream = file.OpenRead())
                using (var sr = new StreamReader(stream))
                using (var jr = new JsonTextReader(sr))
                    return serializer.Deserialize<Profile>(jr);
            }
            else
            {
                Log.Write("Settings", $"Profile '{file.FullName}' does not exist", LogLevel.Warning);
                return null;
            }
        }

        public static void Recover(FileInfo file, Profile profile)
        {
            using (var stream = file.OpenRead())
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
            {
                void propertyWatch(object _, PropertyChangedEventArgs p)
                {
                    var prop = profile.GetType().GetProperty(p.PropertyName).GetValue(profile);
                    Log.Write("Settings", $"Recovered '{p.PropertyName}'", LogLevel.Debug);
                }

                profile.PropertyChanged += propertyWatch;

                try
                {
                    serializer.Populate(jr, profile);
                }
                catch (JsonReaderException e)
                {
                    Log.Write("Settings", $"Recovery ended. Reason: {e.Message}", LogLevel.Debug);
                }

                profile.PropertyChanged -= propertyWatch;
            }
        }

        private static void SerializationErrorHandler(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
        {
            args.ErrorContext.Handled = true;
            if (args.ErrorContext.Path is string path)
            {
                if (args.CurrentObject == null)
                    return;

                var property = args.CurrentObject.GetType().GetProperty(path);
                if (property != null && property.PropertyType == typeof(PluginSettingStore))
                {
                    var match = propertyValueRegex.Match(args.ErrorContext.Error.Message);
                    if (match.Success)
                    {
                        var objPath = ProfileMigrator.MigrateNamespace(match.Groups[1].Value);
                        var newValue = PluginSettingStore.FromPath(objPath);
                        if (newValue != null)
                        {
                            property.SetValue(args.CurrentObject, newValue);
                            Log.Write("Settings", $"Migrated {path} to {nameof(PluginSettingStore)}");
                            return;
                        }
                    }
                }
                Log.Write("Settings", $"Unable to migrate {path}", LogLevel.Error);
                return;
            }
            Log.Exception(args.ErrorContext.Error);
        }

        private static string GetTabletName(TabletHandlerID id)
        {
            return Info.Driver.GetTabletState(id)?.Properties?.Name;
        }
    }
}