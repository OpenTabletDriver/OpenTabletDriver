using System;
using System.Collections.Immutable;

namespace OpenTabletDriver.Desktop.Updater
{
    public record Update
    {
        public Update(Version version, ImmutableArray<string> paths, string binaryDirectory)
        {
            Version = version;
            Paths = paths;
            BinaryDirectory = binaryDirectory;
        }

        public Version Version { get; init; }

        /// <summary>
        /// Gets the paths of files/directories directly in the root of the update.
        /// </summary>
        public ImmutableArray<string> Paths { get; init; }

        /// <summary>
        /// Gets the directory where updated binaries are to be installed.
        /// </summary>
        public string BinaryDirectory { get; init; }
    }
}
