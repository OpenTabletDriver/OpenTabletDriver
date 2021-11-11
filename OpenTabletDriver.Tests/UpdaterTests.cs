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
            var binDir = await TestInstall(new WindowsUpdater());

            var wpfUX = Path.Join(binDir, "OpenTabletDriver.UX.Wpf.exe");
            Assert.True(File.Exists(wpfUX));
        }

        [Fact]
        public async Task TestMacOSInstall()
        {
            var binDir = await TestInstall(new MacOSUpdater());

            var macOSUX = Path.Join(binDir, "OpenTabletDriver.UX.MacOS");
            Assert.True(File.Exists(macOSUX));
        }

        public async Task<string> TestInstall(IUpdater updater)
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

            var latest = await updater.GetLatestRelease();
            await updater.Install(latest, binDir);
            return binDir;
        }

        private static void CleanDirectory(string directory)
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
            Directory.CreateDirectory(directory);
        }
    }
}