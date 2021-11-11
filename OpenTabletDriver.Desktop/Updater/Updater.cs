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
        private Release latestRelease;

        protected static readonly Version CurrentVersion = new Version(typeof(IUpdater).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion);

        public Task<bool> HasUpdate => CheckForUpdates();

        public async Task InstallUpdate(string targetDir)
        {
            await Install(latestRelease ?? await GetLatestRelease(), targetDir);
        }

        private async Task<bool> CheckForUpdates()
        {
            var latestRelease = await GetLatestRelease();
            var latestVersion = new Version(latestRelease.TagName[1..]); // remove `v` from `vW.X.Y.Z
            return latestVersion > CurrentVersion;
        }

        private async Task<Release> GetLatestRelease()
        {
            latestRelease = await github.Repository.Release.GetLatest("OpenTabletDriver", "OpenTabletDriver");
            return latestRelease;
        }

        protected async Task Install(Release release)
        {
            var targetDir = AppDomain.CurrentDomain.BaseDirectory;
            await Install(release, targetDir);
        }

        protected virtual async Task Install(Release release, string targetDir)
        {
            var binaryDir = await Download(release);
            var oldDir = Path.Join(AppInfo.Current.TemporaryDirectory, CurrentVersion + "-old");

            if (Directory.Exists(targetDir))
                Directory.Move(targetDir, oldDir);

            Directory.Move(binaryDir, targetDir);
        }

        protected abstract Task<string> Download(Release release);
        protected abstract ReleaseAsset GetAsset(Release release);
    }
}