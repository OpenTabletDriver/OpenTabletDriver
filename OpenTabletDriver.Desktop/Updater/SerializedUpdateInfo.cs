using System;

namespace OpenTabletDriver.Desktop.Updater
{
    public sealed class SerializedUpdateInfo
    {
        public SerializedUpdateInfo() { }

        public SerializedUpdateInfo(UpdateInfo updateInfo)
        {
            Version = updateInfo.Version;
        }

        public Version Version { get; set; } = new Version();
    }
}
