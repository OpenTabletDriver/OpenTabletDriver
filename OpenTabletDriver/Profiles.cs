using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Numerics;
using Newtonsoft.Json;
using OpenTabletDriver.Binding;
using OpenTabletDriver.Plugin;
using System.Xml.Linq;

namespace OpenTabletDriver
{
    public class Profiles: List<Profile>
    {
        public Profiles() { }

        public static readonly Profiles Current = new Profiles();
            
        public Profiles Load(string ProfileDirectory)
        {
            var profileDir = new DirectoryInfo(ProfileDirectory);
            if (profileDir.Exists)
            {
                foreach (var profileFile in profileDir.EnumerateFiles("*.json", SearchOption.AllDirectories))
                {
                    Log.Write("Profile", $"Loading profile: {profileFile.Name}", LogLevel.Info);
                    var profile = new Profile();
                    Current.Add(profile.Load(profileFile));
                }
            };
            return Current;
        }

        public Profile GetProfile(string ProfileName)
        {
            foreach (Profile profile in this)
            {
                if (profile.ProfileName == ProfileName)
                {
                    return profile;
                }
            }
            return null;
        }

        public void Default()
        {
            if (Current.Count == 0)
            {
                var profile = new Profile();
                profile.Settings = Settings.Defaults;
                profile.ProfileName = "Default";
                profile.ProfileFile = Path.Join(AppInfo.Current.ProfileDirectory, "Default.json");
                Current.Add(profile);
            };
            foreach (Profile profile in Current)
            {
                profile.Settings = Settings.Defaults;
            }
        }
        public void Save()
        {
            foreach (Profile profile in Current)
            {
                profile.Save();
            }
        }

    }

    public class Profile : Notifier
    {
        public Profile() { }
            
        public Profile Load(FileInfo profileFile)
        {
            ProfileName = Path.GetFileNameWithoutExtension(profileFile.Name);
            Settings = Settings.Deserialize(profileFile);
            ProfileFile = profileFile.FullName;
            return this;
        }

        private string _profileName;
        private string _profileFile;
        private Settings _settings;

        public void Save()
        {
            Settings.Serialize(new FileInfo(_profileFile));
        }


        public Settings Settings
        {
            set => RaiseAndSetIfChanged(ref _settings, value != null ? value : null);
            get => _settings;
        }

        public string ProfileName
        {
            set => RaiseAndSetIfChanged(ref _profileName, value != null ? value : "Default");
            get => _profileName;
        }

        public string ProfileFile
        {
            set => RaiseAndSetIfChanged(ref _profileFile, value != null ? value : "");
            get => _profileFile;
        }
    }
}