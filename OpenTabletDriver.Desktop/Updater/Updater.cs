#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTabletDriver.Desktop.Updater
{
    public abstract class Updater : IUpdater
    {
        private static readonly ImmutableArray<string> AppDataFiles =
            ImmutableArray.Create(
                "settings.json",
                "Presets"
            );

        protected static readonly Version AssemblyVersion = typeof(IUpdater).Assembly.GetName().Version!;

        protected Updater(Version currentVersion, string binaryDir, string appDataDir, string rollbackDir)
        {
            CurrentVersion = currentVersion;
            BinaryDirectory = binaryDir;
            RollbackDirectory = rollbackDir;
            AppDataDirectory = appDataDir;

            if (!Directory.Exists(RollbackDirectory))
                Directory.CreateDirectory(RollbackDirectory);
        }

        private int _installing;
        private bool _installed;

        protected Version CurrentVersion { get; }
        protected string BinaryDirectory { get; }
        protected string AppDataDirectory { get; }
        protected string RollbackDirectory { get; }

        public event Action<Update>? UpdateInstalling;
        public event Action<string>? RollbackCreated;
        public event Action<Update>? UpdateInstalled;

        public async Task<UpdateInfo?> CheckForUpdates()
        {
            var update = await CheckForUpdatesCore();
            if (update == null || update.Version <= CurrentVersion)
                return null;

            return update;
        }

        public Task Install(Update update)
        {
            if (_installed)
                throw new UpdateAlreadyInstalledException();

            if (Interlocked.CompareExchange(ref _installing, 1, 0) == 1)
                throw new UpdateInProgressException();

            var installed = false;
            try
            {
                UpdateInstalling?.Invoke(update);
                var rollback = CreateRollback();
                var hasAccess = ProbeAccessToBinaryPath();

                if (!hasAccess)
                {
                    RunElevated(update, rollback.Binary);
                    return Task.CompletedTask;
                }

                // Moves files/directories that would be updated to a rollback directory.
                // Doing a move is necessary for Windows to allow the update to overwrite the files.
                BackupAppData(rollback.AppData);
                Backup(rollback.Binary, update.Paths, File.Move, Directory.Move);

                RollbackCreated?.Invoke(rollback.Directory);

                InstallUpdateFiles(update.Paths, BinaryDirectory);
                installed = true;
            }
            finally
            {
                _installing = 0;
                _installed = installed; // this will be false if PrepareForUpdate or InstallCore throws

                if (installed)
                {
                    UpdateInstalled?.Invoke(update);
                }
            }

            return Task.CompletedTask;
        }

        private bool ProbeAccessToBinaryPath()
        {
            try
            {
                var filePath = Path.Join(BinaryDirectory, ".probe");
                var file = File.OpenWrite(filePath);
                file.Dispose();
                File.Delete(filePath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private static void InstallUpdateFiles<T>(T sources, string destinationDirectory)
            where T : IEnumerable<string>
        {
            foreach (var source in sources)
            {
                var destination = Path.Join(destinationDirectory, Path.GetFileName(source));

                if (File.Exists(source))
                    File.Move(source, destination);
                else if (Directory.Exists(source))
                    Directory.Move(source, destination);
            }
        }

        protected abstract Task<UpdateInfo?> CheckForUpdatesCore();

        protected virtual void RunElevated(Update update, string binaryRollback)
        {
            // throw by default
            throw new UpdateInstallFailedException($"Unauthorized access to '{binaryRollback}' and no elevation method was provided.");
        }

        private Rollback CreateRollback()
        {
            string timestamp = DateTime.UtcNow.ToString("-yyyy-MM-dd_hh-mm-ss");
            var rollback = Path.Join(RollbackDirectory, CurrentVersion + timestamp);
            var binaryRollback = Path.Join(rollback, "bin");
            var appDataRollback = Path.Join(rollback, "appdata");

            Directory.CreateDirectory(rollback);
            Directory.CreateDirectory(binaryRollback);
            Directory.CreateDirectory(appDataRollback);

            return new Rollback(rollback, binaryRollback, appDataRollback);
        }

        public void Backup(
            string binaryRollback,
            ImmutableArray<string> pathsToUpdate,
            Action<string, string> fileAction,
            Action<string, string> directoryAction)
        {
            foreach (var path in pathsToUpdate.Select(f => Path.GetFileName(f)))
            {
                var source = Path.Join(BinaryDirectory, path);
                var destination = Path.Join(binaryRollback, path);

                if (File.Exists(source))
                    fileAction(source, destination);
                else if (Directory.Exists(source))
                    directoryAction(source, destination);
            }
        }

        // Copies important files/directories from AppDataDirectory to a rollback directory.
        protected void BackupAppData(string appDataRollback)
        {
            foreach (var path in AppDataFiles)
            {
                var source = Path.Join(AppDataDirectory, path);
                var destination = Path.Join(appDataRollback, path);

                if (File.Exists(source) || Directory.Exists(source))
                {
                    Copy(source, destination);
                }
            }
        }

        protected static void Copy(string source, string destination)
        {
            // if file
            if (File.Exists(source))
            {
                File.Copy(source, destination, true);
                return;
            }

            // else directory
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            foreach (var path in Directory.EnumerateFileSystemEntries(source))
            {
                var pathName = Path.GetFileName(path);
                var destPath = Path.Join(destination, pathName);
                Copy(path, destPath);
            }
        }

        protected static string GetDownloadPath()
        {
            return Path.Join(Path.GetTempPath(), Path.GetRandomFileName());
        }

        record Rollback(string Directory, string Binary, string AppData);
    }

    public class UpdateException : Exception
    {
        public UpdateException() : base("An error has occurred during update")
        {
        }

        public UpdateException(string msg) : base(msg)
        {
        }

        public UpdateException(string msg, Exception innerException) : base(msg, innerException)
        {
        }
    }

    public class UpdateInProgressException : UpdateException
    {
        public UpdateInProgressException() : base("An update is already in progress.")
        {
        }
    }

    public class UpdateAlreadyInstalledException : UpdateException
    {
        public UpdateAlreadyInstalledException() : base("An update has already been installed.")
        {
        }
    }

    public class UpdateInstallFailedException : UpdateException
    {
        public UpdateInstallFailedException() : base("Failed to install update.")
        {
        }

        public UpdateInstallFailedException(string msg) : base(msg)
        {
        }

        public UpdateInstallFailedException(Exception innerException) : base("Failed to install update.", innerException)
        {
        }

        public UpdateInstallFailedException(string msg, Exception innerException) : base(msg, innerException)
        {
        }
    }
}
