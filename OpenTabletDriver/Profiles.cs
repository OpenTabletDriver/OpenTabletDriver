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
    public class Profiles
    {
        private List<Profile> ProfileList = new List<Profile>();

        public Profiles() {}
            
        public void Load(string ProfileDirectory)
        {
            var profileDir = new DirectoryInfo(ProfileDirectory);
            if (profileDir.Exists)
            {
                foreach (var profileFile in profileDir.EnumerateFiles("*.json", SearchOption.AllDirectories))
                {
                    Log.Write("Profile", $"Loading profile: {profileFile.Name}", LogLevel.Info);
                    ProfileList.Add( new Profile(profileFile));
                }
            };
        }

        public Profile GetProfile(string ProfileName)
        {
            return ProfileList.Where(i => i.ProfileName == ProfileName).FirstOrDefault();
        }

        public void Default()
        {
            foreach (Profile profile in ProfileList)
            {
                profile.Settings = Settings.Defaults;
            }
        }

    }

    public class Profile : Notifier
    {
        public Profile(FileInfo profileFile)
        {
            ProfileName = Path.GetFileNameWithoutExtension(profileFile.Name);
            Settings = Settings.Deserialize(profileFile);
            _profileFile = profileFile;
        }

        private string _profileName;
        private FileInfo _profileFile;
        private Settings _settings;

        public void Save()
        {
            Settings.Serialize(_profileFile);
        }


        public Settings Settings
        {
            set => RaiseAndSetIfChanged(ref _settings, value != null ? value : null);
            get => _settings;
        }

        public string ProfileName
        {
            set => RaiseAndSetIfChanged(ref _profileName, value != "{Disable}" ? value : null);
            get => _profileName;
        }
    }
}