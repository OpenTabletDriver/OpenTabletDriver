using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Interop.AppInfo;

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

        public static async Task<PluginMetadataCollection> DownloadAsync(IAppInfo appInfo, string owner, string name, string gitRef)
        {
            string archiveUrl = $"https://api.github.com/repos/{owner}/{name}/tarball/{gitRef}";
            return await DownloadAsync(archiveUrl, appInfo.CacheDirectory);
        }

        private static async Task<PluginMetadataCollection> DownloadAsync(string archiveUrl, string cacheDir)
        {
            using (var client = GetClient())
            using (var httpStream = await client.GetStreamAsync(archiveUrl))
                return FromStream(httpStream, cacheDir);
        }

        private static PluginMetadataCollection FromStream(Stream stream, string cacheDir)
        {
            var memStream = new MemoryStream();
            stream.CopyTo(memStream);

            using (memStream)
            using (var gzipStream = new GZipInputStream(memStream))
            using (var archive = TarArchive.CreateInputTarArchive(gzipStream, null))
            {
                string hash = CalculateSHA256(memStream);
                string metadataCacheDir = Path.Join(cacheDir, $"{hash}-OpenTabletDriver-PluginMetadata");

                if (Directory.Exists(metadataCacheDir))
                    Directory.Delete(metadataCacheDir, true);
                archive.ExtractContents(metadataCacheDir);

                var collection = EnumeratePluginMetadata(metadataCacheDir);
                var metadataCollection = new PluginMetadataCollection(collection);

                return metadataCollection;
            }
        }

        private static string CalculateSHA256(Stream stream)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashData = sha256.ComputeHash(stream);
                stream.Position = 0;
                var sb = new StringBuilder();
                foreach (var val in hashData)
                {
                    var hex = val.ToString("x2");
                    sb.Append(hex);
                }
                return sb.ToString();
            }
        }

        private static IEnumerable<PluginMetadata> EnumeratePluginMetadata(string directoryPath)
        {
            foreach (var file in Directory.EnumerateFiles(directoryPath, "*.json", SearchOption.AllDirectories))
                using (var fs = File.OpenRead(file))
                    yield return Serialization.Deserialize<PluginMetadata>(fs)!;
        }
    }
}
