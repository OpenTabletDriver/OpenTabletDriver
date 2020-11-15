using System;

namespace OpenTabletDriver.Plugin.Timers
{
    public interface ITimer : IDisposable
    {
        public void Start();
        public void Stop();
        public bool Stop(int milliseconds);
        public bool Enabled { get; }
        public float Interval { get; set; }
        public event Action Elapsed;
    }
}