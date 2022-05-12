using System;
using System.Threading.Tasks;
using Octokit;

#nullable enable

namespace OpenTabletDriver.Desktop.Updater
{
    public abstract class GithubUpdater : Updater<GithubRelease>
    {
        private readonly GitHubClient github = new(new ProductHeaderValue("OpenTabletDriver"));

        protected GithubUpdater(Version? currentVersion, string binaryDir, string appDataDir, string rollbackDir)
            : base(currentVersion, binaryDir, appDataDir, rollbackDir)
        {
        }

        protected override async Task<GithubRelease> GetUpdate()
        {
            var release = await github.Repository.Release.GetLatest("OpenTabletDriver", "OpenTabletDriver");
            var version = new Version(release!.TagName[1..]); // remove `v` from `vW.X.Y.Z
            return new GithubRelease(release, version);
        }
    }

    public record GithubRelease(Release Release, Version Version) : UpdateInfo(Version);
}
