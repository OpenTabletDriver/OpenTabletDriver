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