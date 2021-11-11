using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HidSharp.Reports.Units;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Updater;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class UpdaterTests
    {
        [Fact]
        public async Task TestWindowsInstall()
        {
            await TestInstall(new WindowsUpdater());
        }

        [Fact]
        public async Task TestMacOSInstall()
        {
            await TestInstall(new MacOSUpdater());
        }

        public async Task TestInstall(IUpdater updater)
        {
            string testDir = Path.Join(
                Environment.GetEnvironmentVariable("HOME"),
                "OpenTabletDriver.Tests",
                nameof(UpdaterTests),
                updater.GetType().Name
            );
            string tempDir = Path.Join(testDir, "temp");
            string binDir = Path.Join(testDir, "bin");

            AppInfo.Current = new AppInfo()
            {
                TemporaryDirectory = tempDir
            };

            CleanDirectory(tempDir);

            var latest = await updater.GetLatest();
            await updater.Install(latest, binDir);
        }

        private static void CleanDirectory(string directory)
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
            Directory.CreateDirectory(directory);
        }
    }
}