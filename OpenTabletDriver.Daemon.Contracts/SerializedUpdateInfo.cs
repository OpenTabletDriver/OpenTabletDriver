using System;

namespace OpenTabletDriver.Daemon.Contracts
{
    public sealed class SerializedUpdateInfo
    {
        public SerializedUpdateInfo(Version version)
        {
            Version = version;
        }

        public Version Version { get; }
    }
}
