using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Octokit;
using OpenTabletDriver.Desktop.Updater;
using Xunit;

#nullable enable

namespace OpenTabletDriver.Tests
{
    public class UpdaterTests
    {
        public static TheoryData<Type, string> Updater_InstallsBinary_InCorrectDirectory_Async_Data => new TheoryData<Type, string>()
        {
            { typeof(WindowsUpdater), "OpenTabletDriver.UX.Wpf.exe" },
            { typeof(MacOSUpdater), "OpenTabletDriver.UX.MacOS" }
        };

        [Theory]
        [MemberData(nameof(Updater_InstallsBinary_InCorrectDirectory_Async_Data))]
        public Task Updater_InstallsBinary_InCorrectDirectory_Async(Type updaterType, string expectedUiBinaryName)
        {
            return MockEnvironmentAsync(async (updaterEnv) =>
            {
                var updater = CreateUpdater(updaterType, updaterEnv);

                await updater.InstallUpdate();
                var uiBinaryExists = File.Exists(Path.Join(updaterEnv.BinaryDir, expectedUiBinaryName));
                var daemonBinaryExists = Directory.EnumerateFiles(updaterEnv.BinaryDir!)
                    .FirstOrDefault(f => Path.GetFileName(f).StartsWith("OpenTabletDriver.Daemon")) != null;

                Assert.True(uiBinaryExists, $"UI binary not found in {updaterEnv.BinaryDir}");
                Assert.True(daemonBinaryExists, $"Daemon binary not found in {updaterEnv.BinaryDir}");
            });
        }

        public static TheoryData<Version?, bool> UpdaterBase_ProperlyChecks_Version_Async_Data => new TheoryData<Version?, bool>()
        {
            // Outdated
            { new Version("0.1.0.0"), true },
            // Same version
            { null, false },
            // From the future
            { new Version("99.0.0.0"), false },
        };

        [Theory]
        [MemberData(nameof(UpdaterBase_ProperlyChecks_Version_Async_Data))]
        public Task UpdaterBase_ProperlyChecks_Version_Async(Version version, bool expectedUpdateStatus)
        {
            return MockEnvironmentAsync(async (updaterEnv) =>
            {
                updaterEnv.Version = version;
                var mockUpdater = CreateMockUpdater<Updater>(updaterEnv).Object;

                var hasUpdate = await mockUpdater.HasUpdate;

                Assert.Equal(expectedUpdateStatus, hasUpdate);
            });
        }

        [Theory]
        [MemberData(nameof(UpdaterBase_ProperlyChecks_Version_Async_Data))]
        public Task Updater_PreventsUpdate_WhenAlreadyUpToDate_Async(Version version, bool expectedUpdateStatus)
        {
            return MockEnvironmentAsync(async (updaterEnv) =>
            {
                updaterEnv.Version = version;
                var mockUpdater = CreateMockUpdater<Updater>(updaterEnv);

                // Track calls to Updater.Install
                mockUpdater.Protected()
                    .Setup<Task>("Install", ItExpr.IsAny<Release>())
                    .Returns(Task.CompletedTask)
                    .Verifiable();

                var mockUpdaterObject = mockUpdater.Object;

                await mockUpdaterObject.InstallUpdate();

                // Verify if Updater.Install is called, non-null exception means it wasn't
                var hasInstalledUpdate = Record.Exception(() => mockUpdater.Verify()) == null;
                Assert.Equal(expectedUpdateStatus, hasInstalledUpdate);
            });
        }

        [Fact]
        public Task Updater_Prevents_ConcurrentAndConsecutive_Updates_Async()
        {
            return MockEnvironmentAsync(async (updaterEnv) =>
            {
                var mockUpdater = CreateMockUpdater<Updater>(updaterEnv);
                var callCount = 0;

                // Track call count of Updater.Install
                mockUpdater.Protected()
                    .Setup<Task>("Install", ItExpr.IsAny<Release>())
                    .Returns(Task.CompletedTask)
                    .Callback(() => callCount++);

                var mockUpdaterObject = mockUpdater.Object;
                var parallelTasks = new Task[]
                {
                    Task.Run(() => mockUpdaterObject.InstallUpdate()),
                    Task.Run(() => mockUpdaterObject.InstallUpdate()),
                    Task.Run(() => mockUpdaterObject.InstallUpdate()),
                    Task.Run(() => mockUpdaterObject.InstallUpdate())
                };

                await Task.WhenAll(parallelTasks);

                Assert.Equal(1, callCount);
            });
        }

        private static void InitializeDirectory(string directory)
        {
            CleanDirectory(directory);
            Directory.CreateDirectory(directory);
        }

        private static void CleanDirectory(string directory)
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }

        private static async Task MockEnvironmentAsync(Func<UpdaterEnvironment, Task> asyncAction)
        {
            var mockUpdaterEnv = new UpdaterEnvironment()
            {
                Version = new Version("0.1.0.0"),
                BinaryDir = Path.Join(Path.GetTempPath(), Path.GetRandomFileName()),
                AppDataDir = Path.Join(Path.GetTempPath(), Path.GetRandomFileName())
            };

            mockUpdaterEnv.RollBackDir = Path.Join(mockUpdaterEnv.AppDataDir, "Temp");

            InitializeDirectory(mockUpdaterEnv.BinaryDir);
            InitializeDirectory(mockUpdaterEnv.AppDataDir);
            InitializeDirectory(mockUpdaterEnv.RollBackDir);

            try
            {
                await asyncAction(mockUpdaterEnv);
            }
            finally
            {
                CleanDirectory(mockUpdaterEnv.BinaryDir);
                CleanDirectory(mockUpdaterEnv.AppDataDir);
                CleanDirectory(mockUpdaterEnv.RollBackDir);
            }
        }

        private static Mock<T> CreateMockUpdater<T>(UpdaterEnvironment updaterEnv) where T : class, IUpdater
        {
            var mockUpdater = new Mock<T>(updaterEnv.Version, updaterEnv.BinaryDir, updaterEnv.AppDataDir, updaterEnv.RollBackDir) { CallBase = true };
            return mockUpdater;
        }

        private static IUpdater CreateUpdater(Type updaterType, UpdaterEnvironment updaterEnvironment)
        {
            if (updaterEnvironment.Version == null)
                return (IUpdater)Activator.CreateInstance(updaterType)!;

            return (IUpdater)Activator.CreateInstance(
                updaterType,
                updaterEnvironment.Version,
                updaterEnvironment.BinaryDir,
                updaterEnvironment.AppDataDir,
                updaterEnvironment.RollBackDir
            )!;
        }

        private struct UpdaterEnvironment
        {
            public Version? Version;
            public string? BinaryDir;
            public string? AppDataDir;
            public string? RollBackDir;
        }
    }
}