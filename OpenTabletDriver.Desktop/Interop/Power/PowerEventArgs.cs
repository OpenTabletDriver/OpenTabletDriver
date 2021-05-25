    using System;

namespace OpenTabletDriver.Desktop.Interop.Power 
{
    public class PowerEventArgs : EventArgs 
    {
        public PowerEventType EventType;

        public PowerEventArgs(PowerEventType type) => EventType = type;
    }
}
