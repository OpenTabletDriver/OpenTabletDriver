using System;
using System.IO;
using System.Threading.Tasks;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Updater;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class UpdaterTests
    {
        public static TheoryData<IUpdater, string> Updater_TestInstall_Data => new TheoryData<IUpdater, string>()
        {
            { new WindowsUpdater(), "OpenTabletDriver.UX.Wpf.exe" },
            { new MacOSUpdater(), "OpenTabletDriver.UX.MacOS" }
        };

        [Theory]
        [MemberData(nameof(Updater_TestInstall_Data))]
        public async Task Updater_TestInstall(IUpdater updater, string expectedBinary)
        {
            var binDir = await TestInstall(updater);

            var binaryPath = Path.Join(binDir, expectedBinary);

            Assert.True(File.Exists(binaryPath));
        }

        public static TheoryData<IUpdater, bool> Updater_ProperlyChecks_Version_Data => new TheoryData<IUpdater, bool>()
        {
            // Outdated
            { new WindowsUpdater(new Version("0.1.0.0")), true },
            { new MacOSUpdater(new Version("0.1.0.0")), true },
            // Updated
            { new WindowsUpdater(), false },
            { new MacOSUpdater(), false },
            // From the future
            { new WindowsUpdater(new Version("99.0.0.0")), false },
            { new MacOSUpdater(new Version("99.0.0.0")), false }
        };

        [Theory]
        [MemberData(nameof(Updater_ProperlyChecks_Version_Data))]
        public async Task Updater_ProperlyChecks_Version(IUpdater updater, bool expectedUpdateStatus)
        {
            var hasUpdate = await updater.HasUpdate;

            Assert.Equal(expectedUpdateStatus, hasUpdate);
        }

        private async Task<string> TestInstall(IUpdater updater)
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

            await updater.InstallUpdate(binDir);
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