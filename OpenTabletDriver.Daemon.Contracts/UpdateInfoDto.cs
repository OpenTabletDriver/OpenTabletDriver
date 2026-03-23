using System;

namespace OpenTabletDriver.Daemon.Contracts
{
    public sealed class UpdateInfoDto
    {
        public UpdateInfoDto(Version version)
        {
            Version = version;
        }

        public Version Version { get; }
    }
}
