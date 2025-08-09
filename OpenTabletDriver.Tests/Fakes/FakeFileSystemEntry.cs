using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace OpenTabletDriver.Tests.Fakes
{
    public abstract class FakeFileSystemEntry
    {
        public string Name { get; init; } = null!;

        public static async Task SetupFilesAsync(string directory, IEnumerable<FakeFileSystemEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (entry is FakeDirectory dir)
                {
                    var dirPath = Path.Join(directory, dir.Name);
                    Directory.CreateDirectory(dirPath);
                    await SetupFilesAsync(dirPath, dir.Entries);
                }
                else if (entry is FakeFile file)
                {
                    var filePath = Path.Join(directory, file.Name);
                    await File.WriteAllBytesAsync(filePath, file.Data);
                }
            }
        }

        public static async Task VerifyFilesAsync(string directory, IEnumerable<FakeFileSystemEntry> entries, bool strict = true)
        {
            var entriesCache = entries.ToArray();
            var dirEntries = Directory.GetFileSystemEntries(directory);
            var dirEntriesMap = dirEntries.ToDictionary(e => Path.GetFileName(e));

            if (strict && (entriesCache.Length != dirEntries.Length))
                throw new XunitException($"Expected {entriesCache.Length} file system entries, got {dirEntries.Length}");

            foreach (var entry in entriesCache)
            {
                if (!dirEntriesMap.TryGetValue(entry.Name, out var path))
                    throw new XunitException($"Expected file system entry '{entry.Name}' not found");

                if (entry is FakeDirectory dir)
                {
                    if (!Directory.Exists(path))
                        throw new DirectoryNotFoundException($"Directory '{path}' does not exist.");

                    await VerifyFilesAsync(path, dir.Entries);
                }
                else if (entry is FakeFile file)
                {
                    if (!File.Exists(path))
                        throw new FileNotFoundException($"File '{path}' does not exist.");

                    var data = await File.ReadAllBytesAsync(path);
                    if (!data.AsSpan().SequenceEqual(file.Data))
                        throw new Exception($"File {path} does not match expected data");
                }
            }
        }
    }
}
