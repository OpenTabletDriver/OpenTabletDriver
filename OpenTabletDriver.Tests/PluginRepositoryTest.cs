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
        private static string AppDataDirectory => Path.Join(TestDirectory, nameof(PluginRepositoryTest));
        private static string CollectionTarball => Path.Join(AppInfo.Current.AppDataDirectory, "repository.tar.gz");

        [TestInitialize]
        public void Initialize()
        {
            AppInfo.Current = new AppInfo
            {
                AppDataDirectory = AppDataDirectory
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            string dir = AppInfo.Current.CacheDirectory;
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }

        [TestMethod]
        public async Task ExpandRepositoryTarball()
        {
            foreach (var metadata in await GetRepository())
            {
                Console.WriteLine();
                Console.Out.WriteProperties(metadata);
            }
        }

        protected async Task<PluginMetadataCollection> GetRepository()
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