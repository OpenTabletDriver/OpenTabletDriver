using System;

namespace OpenTabletDriver.Plugin.Timers
{
    public interface ITimer : IDisposable
    {
        void Start();
        void Stop();
        bool Enabled { get; }
        float Interval { get; set; }
        event Action Elapsed;
    }
}
