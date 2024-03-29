using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenTabletDriver.Daemon.Updater;
using OpenTabletDriver.Tests.Fakes;
using OpenTabletDriver.Tests.Updater;
using Xunit;

#pragma warning disable VSTHRD200

namespace OpenTabletDriver.Tests
{
    public class UpdaterTests
    {
        // The default update version of the mock updater
        private static readonly Version MockVersion = new(1, 0);

        public static TheoryData<Version?, bool> CheckForUpdates_Returns_Update_When_Available_Data => new()
        {
            // These are versions of the update, not the driver

            // Outdated
            { new Version("0.1.0.0"), false },
            // Same version
            { MockVersion, false },
            // From the future
            { new Version("99.0.0.0"), true },
        };

        [Theory]
        [MemberData(nameof(CheckForUpdates_Returns_Update_When_Available_Data))]
        public async Task CheckForUpdates_Returns_Update_When_Available(Version version, bool expectedUpdateStatus)
        {
            using var mockEnv = new UpdaterEnvironment(MockVersion);
            var mockUpdater = new FakeUpdater(mockEnv);

            await mockUpdater.CreateUpdateAsync(version);
            var hasUpdate = await mockUpdater.CheckForUpdates() is not null;

            Assert.Equal(expectedUpdateStatus, hasUpdate);
        }

        [Fact]
        public async Task Install_Throws_UpdateAlreadyInstalledException_When_AlreadyInstalled()
        {
            await MockNormalEnvironment(async (mockUpdater, update) =>
            {
                await mockUpdater.Install(update);
                await Assert.ThrowsAsync<UpdateAlreadyInstalledException>(() => mockUpdater.Install(update));
            });
        }

        [Fact]
        public async Task Install_DoesNotThrow_UpdateAlreadyInstalledException_When_PreviousInstallFailed()
        {
            await MockNormalEnvironment(async (mockUpdater, update) =>
            {
                await mockUpdater.SimulateFailedInstallAsync<Exception>(update);
                await mockUpdater.Install(update);
            });
        }

        [Fact]
        public async Task Install_Throws_UpdateInProgressException_When_AnotherUpdate_Is_InProgress()
        {
            await MockNormalEnvironment(async (mockUpdater, update) =>
            {
                using var originalUpdateFinished = new ManualResetEvent(false);
                using var originalUpdateBlocking = new ManualResetEvent(false);

                mockUpdater.UpdateInstalling += wait; // control when the update will finish to make the test consistent
                var installTask = Task.Run(async () => await mockUpdater.Install(update)); // explicitly run on separate thread
                originalUpdateBlocking.WaitOne();  // make sure that the install task already started
                mockUpdater.UpdateInstalling -= wait; // remove the wait event so the next call won't call wait

                await Assert.ThrowsAsync<UpdateInProgressException>(() => mockUpdater.Install(update));

                originalUpdateFinished.Set();
                await installTask;

                void wait(Update update)
                {
                    originalUpdateBlocking.Set();
                    originalUpdateFinished.WaitOne();
                }
            });
        }

        [Fact]
        public async Task Install_Moves_UpdatedBinaries_To_BinDirectory()
        {
            var oldFile = new FakeFile()
            {
                Name = "test.bin",
                Data = new byte[] { 0x00, 0x01, 0x02, 0x03 }
            };
            var updatedFile = new FakeFile()
            {
                Name = "test.bin",
                Data = new byte[] { 0x08, 0x09, 0x0A, 0x0B }
            };

            using var mockEnv = new UpdaterEnvironment(MockVersion)
            {
                BinaryFiles =
                {
                    oldFile
                }
            };
            await mockEnv.SetupFilesAsync();

            var mockUpdater = new FakeUpdater(mockEnv)
            {
                UpdateFiles =
                {
                    updatedFile
                }
            };

            var update = await GetUpdate(mockUpdater);
            await mockUpdater.Install(update);

            // verify that the old file was replaced
            await FakeFileSystemEntry.VerifyFilesAsync(mockEnv.BinaryDir, new FakeFileSystemEntry[]
            {
                updatedFile
            });

            // verify that the old file was backed up
            var binRollbackDir = Path.Join(mockEnv.VersionedRollbackDirectory, "bin");
            await FakeFileSystemEntry.VerifyFilesAsync(binRollbackDir, new FakeFileSystemEntry[]
            {
                oldFile
            });
        }

        [Fact]
        public async Task Install_Moves_Only_ToBeUpdated_Binaries()
        {
            var oldFile = new FakeFile()
            {
                Name = "test.bin",
                Data = new byte[] { 0x00, 0x01, 0x02, 0x03 }
            };
            var extraFile = new FakeFile()
            {
                Name = "extra.bin",
                Data = new byte[] { 0x04, 0x05, 0x06, 0x07 }
            };
            var updatedFile = new FakeFile()
            {
                Name = "test.bin",
                Data = new byte[] { 0x08, 0x09, 0x0A, 0x0B }
            };

            using var mockEnv = new UpdaterEnvironment(MockVersion)
            {
                BinaryFiles =
                {
                    oldFile,
                    extraFile
                }
            };
            await mockEnv.SetupFilesAsync();

            var mockUpdater = new FakeUpdater(mockEnv)
            {
                UpdateFiles =
                {
                    updatedFile
                }
            };

            var update = await GetUpdate(mockUpdater);
            await mockUpdater.Install(update);

            // verify that the old file was replaced and the extra file was not
            await FakeFileSystemEntry.VerifyFilesAsync(mockEnv.BinaryDir, new FakeFileSystemEntry[]
            {
                updatedFile,
                extraFile
            });

            // verify that the old file was backed up
            var binRollbackDir = Path.Join(mockEnv.VersionedRollbackDirectory, "bin");
            await FakeFileSystemEntry.VerifyFilesAsync(binRollbackDir, new FakeFileSystemEntry[]
            {
                oldFile
            });
        }

        [Fact]
        public async Task Install_Copies_AppDataFiles()
        {
            var presets = new FakeDirectory
            {
                Name = "Presets",
                Entries =
                {
                    new FakeFile()
                    {
                        Name = "config1.json",
                        Data = Encoding.UTF8.GetBytes("test")
                    },
                    new FakeFile()
                    {
                        Name = "config2.json",
                        Data = Encoding.UTF8.GetBytes("test2")
                    }
                }
            };
            var settings = new FakeFile
            {
                Name = "settings.json",
                Data = Encoding.UTF8.GetBytes("test3")
            };

            using var mockEnv = new UpdaterEnvironment(MockVersion)
            {
                AppDataFiles =
                {
                    presets,
                    settings
                }
            };
            await mockEnv.SetupFilesAsync();

            var mockUpdater = new FakeUpdater(mockEnv);
            var update = await GetUpdate(mockUpdater);
            await mockUpdater.Install(update);

            var appDataRollbackDir = Path.Join(mockEnv.VersionedRollbackDirectory, "appdata");
            var expectedFiles = new FakeFileSystemEntry[]
            {
                presets,
                settings
            };

            // verify that the files were copied to the rollback directory
            await FakeFileSystemEntry.VerifyFilesAsync(appDataRollbackDir, expectedFiles);
            await FakeFileSystemEntry.VerifyFilesAsync(mockEnv.AppDataDir, expectedFiles, strict: false);
        }

        private static async Task MockNormalEnvironment(Func<FakeUpdater, Update, Task> action)
        {
            using var mockEnv = new UpdaterEnvironment(MockVersion);
            var mockUpdater = new FakeUpdater(mockEnv);
            var update = await GetUpdate(mockUpdater);

            await action(mockUpdater, update);
        }

        private static async Task<Update> GetUpdate(FakeUpdater mockUpdater, Version? version = null)
        {
            await mockUpdater.CreateUpdateAsync(version ?? new Version("99.99.99.99"));
            var updateInfo = await mockUpdater.CheckForUpdates();
            return await updateInfo!.GetUpdate();
        }
    }
}
