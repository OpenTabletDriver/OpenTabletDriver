using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using OpenTabletDriver.Daemon.Contracts;

namespace OpenTabletDriver.Daemon
{
    public class PresetManager : IPresetManager
    {
        private readonly string _dir;

        private const string FILE_EXTENSION = ".json";
        private const string FILE_FILTER = $"*{FILE_EXTENSION}";

        public PresetManager(AppInfo appInfo)
        {
            _dir = appInfo.PresetDirectory!;
        }

        public IReadOnlyCollection<string> GetPresets() => EnumerateDir().ToImmutableList();

        public Preset? LoadPreset(string name)
        {
            var path = Path.Join(_dir, name + FILE_EXTENSION);
            var file = new FileInfo(path);

            if (!file.Exists)
                return null;

            var settings = Serialization.Deserialize<Settings>(file)!;
            return new Preset(name, settings);
        }

        public void Save(string name, Settings settings)
        {
            var dir = new DirectoryInfo(_dir);
            if (!dir.Exists)
                dir.Create();

            var files = dir.EnumerateFiles(FILE_FILTER);
            var oldFile = files.FirstOrDefault(f => f.Name.Replace(FILE_EXTENSION, string.Empty) == name);
            oldFile?.Delete();

            var path = Path.Join(dir.FullName, name + FILE_EXTENSION);
            Serialization.Serialize(File.Create(path), settings);
        }

        private IEnumerable<string> EnumerateDir()
        {
            if (!Directory.Exists(_dir))
                return Array.Empty<string>();

            return Directory.EnumerateFiles(_dir, FILE_FILTER, SearchOption.AllDirectories)
                .Select(file => Path.GetFileName(file).Replace(FILE_EXTENSION, string.Empty));
        }
    }
}
