using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Newtonsoft.Json;
using Octokit;

namespace OpenTabletDriver.Desktop.Reflection.Metadata
{
    public class PluginMetadataCollection : Collection<PluginMetadata>
    {
        [JsonConstructor]
        protected PluginMetadataCollection()
        {
        }

        protected PluginMetadataCollection(IEnumerable<PluginMetadata> source)
            : base(source.ToList())
        {
        }

        public const string REPOSITORY_OWNER = "OpenTabletDriver";
        public const string REPOSITORY_NAME = "Plugin-Repository";

        protected static GitHubClient GitHub { get; } = new GitHubClient(new ProductHeaderValue("OpenTabletDriver"));

        internal static HttpClient GetClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "OpenTabletDriver");
            return client;
        }

        public static async Task<PluginMetadataCollection> DownloadAsync()
        {
            string archiveUrl = $"https://api.github.com/repos/{REPOSITORY_OWNER}/{REPOSITORY_NAME}/tarball/";
            using (var client = GetClient())
            using (var httpStream = await client.GetStreamAsync(archiveUrl))
                return FromStream(httpStream);
        }

        public static PluginMetadataCollection FromStream(Stream stream)
        {
            using (var gzipStream = new GZipInputStream(stream))
            using (var archive = TarArchive.CreateInputTarArchive(gzipStream, null))
            {
                string cacheDir = Path.Join(AppInfo.Current.CacheDirectory, REPOSITORY_NAME);
                archive.ExtractContents(cacheDir);
                var collection = EnumeratePluginMetadata(cacheDir);
                return new PluginMetadataCollection(collection);
            }
        }

        protected static IEnumerable<PluginMetadata> EnumeratePluginMetadata(string directoryPath)
        {
            foreach (var file in Directory.EnumerateFiles(directoryPath, "*.json", SearchOption.AllDirectories))
                using (var fs = File.OpenRead(file))
                    yield return Serialization.Deserialize<PluginMetadata>(fs);
        }
    }
}