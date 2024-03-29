using System;
using System.Threading.Tasks;

namespace OpenTabletDriver.Daemon.Updater
{
    public interface IUpdater
    {
        /// <summary>
        /// Checks if an update is available.
        /// </summary>
        /// <returns>
        /// Returns an <see cref="UpdateInfo"/> if an update is available, otherwise returns null.
        /// </returns>
        Task<UpdateInfo?> CheckForUpdates();

        /// <summary>
        /// Installs an update.
        /// </summary>
        /// <param name="update">The update to install.</param>
        /// <returns>The result of installing the update.</returns>
        Task Install(Update update);

        /// <summary>
        /// Occurs when an update is being installed.
        /// </summary>
        event Action<Update> UpdateInstalling;

        /// <summary>
        /// Occurs when a rollback is created. This event provides the path to the rollback directory.
        /// </summary>
        event Action<string> RollbackCreated;

        /// <summary>
        /// Occurs when an update is installed.
        /// </summary>
        event Action<Update> UpdateInstalled;
    }
}
