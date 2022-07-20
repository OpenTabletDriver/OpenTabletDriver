using System;
using System.Threading.Tasks;
using Octokit;
using OpenTabletDriver.Desktop.Interop.AppInfo;

#nullable enable

namespace OpenTabletDriver.Desktop.Updater
{
    public abstract class GitHubUpdater : Updater
    {
        private readonly IGitHubClient _github;

        protected GitHubUpdater(Version? currentVersion, IAppInfo appInfo, IGitHubClient client)
            : base(currentVersion, appInfo.BinaryDirectory, appInfo.AppDataDirectory, appInfo.BackupDirectory)
        {
            _github = client;
        }

        protected override async Task<UpdateInfo?> GetUpdate()
        {
            var release = await _github.Repository.Release.GetLatest("OpenTabletDriver", "OpenTabletDriver");
            var version = new Version(release!.TagName[1..]); // remove `v` from `vW.X.Y.Z
            return new GitHubRelease(release, version);
        }
    }

    public record GitHubRelease(Release Release, Version Version) : UpdateInfo(Version);
}
