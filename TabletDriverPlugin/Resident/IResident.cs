using System;

namespace TabletDriverPlugin.Resident
{
    /// <summary>
    /// A plugin that will be started up and kept running until OpenTabletDriver is closed.
    /// </summary>
    public interface IResident : IDisposable
    {
        bool Initialize();
    }
}