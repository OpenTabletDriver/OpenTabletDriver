using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Reflection.Metadata;

namespace OpenTabletDriver.Tests
{
    [TestClass]
    public class PluginRepositoryTest : TestBase
    {
        private static string TestAppDataDirectory => Path.Join(TestDirectory, nameof(PluginRepositoryTest));
        private static string CollectionTarball => Path.Join(AppInfo.Current.AppDataDirectory, TarballName);
        private const string TarballName = "repository.tar.gz";

        [TestInitialize]
        public void Initialize()
        {
            AppInfo.Current = new AppInfo
            {
                AppDataDirectory = TestAppDataDirectory
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            var directories = new string[]
            {
                AppInfo.Current.CacheDirectory,
                AppInfo.Current.TemporaryDirectory,
                AppInfo.Current.TrashDirectory
            };

            foreach (var dir in directories)
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
        }

        [TestMethod]
        public async Task ExpandRepositoryTarball()
        {
            foreach (var metadata in await GetTestRepository())
            {
                Console.WriteLine();
                Console.Out.WriteProperties(metadata);
            }
        }

        [TestMethod]
        public async Task ExpandRepositoryTarballFork()
        {
            var metadataCollection = await PluginMetadataCollection.DownloadAsync("OpenTabletDriver", PluginMetadataCollection.REPOSITORY_NAME);
            foreach (var metadata in metadataCollection)
            {
                Console.WriteLine();
                Console.Out.WriteProperties(metadata);
            }
        }

        protected async Task<PluginMetadataCollection> GetTestRepository()
        {
            if (File.Exists(CollectionTarball))
            {
                using (var stream = File.OpenRead(CollectionTarball))
                    return PluginMetadataCollection.FromStream(stream);
            }
            else
            {
                return await PluginMetadataCollection.DownloadAsync();
            }
        }
    }
}
