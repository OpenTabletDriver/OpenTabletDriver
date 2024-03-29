using System;
using System.Threading.Tasks;
using Octokit;
using OpenTabletDriver.Daemon.Contracts;

namespace OpenTabletDriver.Daemon.Updater
{
    public abstract class GitHubUpdater : Updater
    {
        private readonly IGitHubClient _github;

        protected GitHubUpdater(Version currentVersion, AppInfo appInfo, IGitHubClient client)
            : base(currentVersion, appInfo.BinaryDirectory, appInfo.AppDataDirectory, appInfo.BackupDirectory)
        {
            _github = client;
        }

        protected abstract Task<Update> Download(Release release, Version version);

        protected override async Task<UpdateInfo?> CheckForUpdatesCore()
        {
            try
            {
                var release = await _github.Repository.Release.GetLatest("OpenTabletDriver", "OpenTabletDriver");
                var version = new Version(release!.TagName[1..]); // remove `v` from `vW.X.Y.Z

                return new UpdateInfo(async () => await Download(release, version))
                {
                    Version = version
                };
            }
            catch (Exception ex)
            {
                Log.Write(nameof(GitHubUpdater), $"Failed to check for updates: {ex}", LogLevel.Error);
                return null;
            }
        }
    }
}
