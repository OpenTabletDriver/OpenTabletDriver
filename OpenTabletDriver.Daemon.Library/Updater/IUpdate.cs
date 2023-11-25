using System;
using System.Collections.Immutable;

namespace OpenTabletDriver.Daemon.Updater
{
    public record Update
    {
        public Update(Version version, ImmutableArray<string> paths)
        {
            Version = version;
            Paths = paths;
        }

        public Version Version { get; init; }

        /// <summary>
        /// Gets the paths of files/directories directly in the root of the update.
        /// </summary>
        public ImmutableArray<string> Paths { get; init; }
    }
}
