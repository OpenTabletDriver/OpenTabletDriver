using JetBrains.Annotations;

namespace OpenTabletDriver.Platform.Environment
{
    /// <summary>
    /// A system environment handler.
    /// This is used to open files in the shell.
    /// </summary>
    [PublicAPI]
    public interface IEnvironmentHandler
    {
        /// <summary>
        /// Opens a file at the specified path.
        /// </summary>
        /// <param name="path">
        /// The path to open.
        /// </param>
        void Open(string path);

        /// <summary>
        /// Opens a folder at the specified path.
        /// </summary>
        /// <param name="path">
        /// The path to open.
        /// </param>
        void OpenFolder(string path);
    }
}
