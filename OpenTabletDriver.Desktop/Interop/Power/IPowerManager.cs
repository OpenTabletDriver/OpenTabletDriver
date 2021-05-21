using System;

namespace OpenTabletDriver.Desktop.Interop.Power 
{
    public interface IPowerManager : IDisposable
    {
    event EventHandler<PowerEventArgs> PowerEvent;
    }
}