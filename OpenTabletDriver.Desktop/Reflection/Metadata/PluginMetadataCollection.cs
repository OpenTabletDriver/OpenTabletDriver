using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Newtonsoft.Json;
using OpenTabletDriver.Plugin;

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

        public static PluginMetadataCollection Empty => new PluginMetadataCollection();

        internal static HttpClient GetClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "OpenTabletDriver");
            return client;
        }

        public static async Task<PluginMetadataCollection> DownloadAsync()
        {
            return await DownloadAsync(REPOSITORY_OWNER, REPOSITORY_NAME);
        }

        public static async Task<PluginMetadataCollection> DownloadAsync(string owner, string name, string gitRef = "")
        {
            string archiveUrl = $"https://api.github.com/repos/{owner}/{name}/tarball/{gitRef}";
            return await DownloadAsync(archiveUrl);
        }

        public static async Task<PluginMetadataCollection> DownloadAsync(string archiveUrl)
        {
            using (var client = GetClient())
            await using (var httpStream = await client.GetStreamAsync(archiveUrl))
                return FromStream(httpStream);
        }

        public static PluginMetadataCollection FromStream(Stream stream)
        {
            var memStream = new MemoryStream();
            stream.CopyTo(memStream);

            using (memStream)
            using (var gzipStream = new GZipInputStream(memStream))
            using (var archive = TarArchive.CreateInputTarArchive(gzipStream, null))
            {
                memStream.Position = 0; // must reset position or SHA256 calculations fail

                string hash = memStream.GetSHA256().PrintHex();
                Debug.Assert(hash != "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"); // empty sha256

                string cacheDir = Path.Join(AppInfo.Current.CacheDirectory, $"{hash}-OpenTabletDriver-PluginMetadata");

                if (Directory.Exists(cacheDir))
                    Directory.Delete(cacheDir, true);

                Debug.Assert(memStream.Position == 0); // archive extraction fails if we're not rewound at this point (!)
                archive.ExtractContents(cacheDir);

                var collection = EnumeratePluginMetadata(cacheDir);
                var metadataCollection = new PluginMetadataCollection(collection);

                return metadataCollection;
            }
        }

        protected static IEnumerable<PluginMetadata> EnumeratePluginMetadata(string directoryPath)
        {
            foreach (var file in Directory.EnumerateFiles(directoryPath, "*.json", SearchOption.AllDirectories))
            {
                using var fs = File.OpenRead(file);

                var metadata = Serialization.Deserialize<PluginMetadata>(fs);

                if (metadata == null)
                {
                    Log.Write(nameof(PluginMetadataCollection), $"Invalid {nameof(PluginMetadata)} file: '{fs.Name}'");
                    continue;
                }

                yield return metadata;
            }
        }
    }
}
