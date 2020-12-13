using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Octokit;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginMetadataCollection : Collection<PluginMetadata>
    {
        [JsonConstructor]
        protected PluginMetadataCollection()
        {
        }

        public const string REPOSITORY_OWNER = "InfinityGhost";
        public const string REPOSITORY_NAME = "OpenTabletDriver-Plugins";

        protected static GitHubClient GitHub { get; } = new GitHubClient(new ProductHeaderValue("OpenTabletDriver"));

        public static async Task<PluginMetadataCollection> DownloadAsync()
        {
            var content = await GitHub.Repository.Content.GetAllContents(REPOSITORY_OWNER, REPOSITORY_NAME, "repository.json");
            var repositorySourceFile = content.FirstOrDefault();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("user-agent", "OpenTabletDriver");
                using (var stream = await client.GetStreamAsync(repositorySourceFile.DownloadUrl))
                    return Serialization.Deserialize<PluginMetadataCollection>(stream);
            }
        }
    }
}