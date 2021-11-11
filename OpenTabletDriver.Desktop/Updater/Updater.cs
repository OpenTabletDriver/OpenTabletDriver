using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Octokit;

namespace OpenTabletDriver.Desktop.Updater
{
    public abstract class Updater : IUpdater
    {
        private GitHubClient github = new GitHubClient(new ProductHeaderValue("OpenTabletDriver"));

        protected string CurrentVersion => "v" + typeof(Updater).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        public virtual async Task<Release> GetLatestRelease()
        {
            return await github.Repository.Release.GetLatest("OpenTabletDriver", "OpenTabletDriver");
        }

        public virtual async Task Install(Release release)
        {
            var targetDir = AppDomain.CurrentDomain.BaseDirectory;
            await Install(release, targetDir);
        }

        public virtual async Task Install(Release release, string targetDir)
        {
            var binaryDir = await Download(release);
            var oldDir = Path.Join(AppInfo.Current.TemporaryDirectory, CurrentVersion + "-old");

            if (Directory.Exists(targetDir))
                Directory.Move(targetDir, oldDir);

            Directory.Move(binaryDir, targetDir);
        }

        public abstract Task<string> Download(Release release);
        public abstract ReleaseAsset GetAsset(Release release);
    }
}