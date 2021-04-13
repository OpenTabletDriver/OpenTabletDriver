using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
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
            : base()
        {
        }

        protected PluginMetadataCollection(IEnumerable<PluginMetadata> source)
            : base(source.ToList())
        {
        }

        public const string REPOSITORY_OWNER = "OpenTabletDriver";
        public const string REPOSITORY_NAME = "Plugin-Repository";
        public const string GIT_REF_REGEX = @"(.+?):(.+$)";

        protected static GitHubClient GitHub { get; } = new GitHubClient(new ProductHeaderValue("OpenTabletDriver"));

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

        public static async Task<PluginMetadataCollection> DownloadAsync(string branchRef, string repoName)
        {
            var match = Regex.Match(branchRef, GIT_REF_REGEX);
            string owner = match.Success ? match.Groups[1].Value : branchRef;
            string gitRef = match.Success ? match.Groups[2].Value : null;

            string archiveUrl = $"https://api.github.com/repos/{owner}/{repoName}/tarball/{gitRef}";
            return await DownloadAsync(archiveUrl);
        }

        public static async Task<PluginMetadataCollection> DownloadAsync(string archiveUrl)
        {
            using (var client = GetClient())
            using (var httpStream = await client.GetStreamAsync(archiveUrl))
                return FromStream(httpStream);
        }

        public static PluginMetadataCollection FromStream(Stream stream)
        {
            using (var gzipStream = new GZipInputStream(stream))
            using (var archive = TarArchive.CreateInputTarArchive(gzipStream, null))
            {
                // TODO: Properly cache instead of storing in the temporary directory
                string cacheDir = Path.Join(AppInfo.Current.TemporaryDirectory, Guid.NewGuid().ToString());

                archive.ExtractContents(cacheDir);
                var collection = EnumeratePluginMetadata(cacheDir);
                var metadataCollection = new PluginMetadataCollection(collection);

                if (Directory.Exists(cacheDir))
                    Directory.Delete(cacheDir, true);

                return metadataCollection;
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
