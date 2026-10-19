using System;
using System.IO;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class SettingsSerializationTests : IDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), $"otd-settings-test-{Guid.NewGuid():N}");

        public SettingsSerializationTests()
        {
            Directory.CreateDirectory(directory);
        }

        [Fact]
        public void Serialize_CreatesAndReplacesSettings()
        {
            var file = new FileInfo(Path.Combine(directory, "settings.json"));
            var settings = new Settings { Revision = "first" };

            settings.Serialize(file);
            Assert.True(Settings.TryDeserialize(file, out var first));
            Assert.Equal("first", first.Revision);

            settings.Revision = "second";
            settings.Serialize(file);
            Assert.True(Settings.TryDeserialize(file, out var second));
            Assert.Equal("second", second.Revision);
            Assert.Single(Directory.GetFiles(directory));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Serialize_FailurePreservesExistingFileAndRemovesTemporaryFile(bool existingFile)
        {
            var file = new FileInfo(Path.Combine(directory, "settings.json"));
            const string original = "{\"Revision\":\"original\"}";
            if (existingFile)
                File.WriteAllText(file.FullName, original);

            Assert.Throws<JsonSerializationException>(() => new FailingSettings().Serialize(file));

            Assert.Equal(existingFile, File.Exists(file.FullName));
            if (existingFile)
                Assert.Equal(original, File.ReadAllText(file.FullName));
            Assert.Equal(existingFile ? 1 : 0, Directory.GetFiles(directory).Length);
        }

        private sealed class FailingSettings : Settings
        {
            public string Failure => throw new InvalidOperationException("Simulated serialization failure");
        }

        public void Dispose()
        {
            foreach (var file in Directory.GetFiles(directory))
                File.Delete(file);
            Directory.Delete(directory);
        }
    }
}
