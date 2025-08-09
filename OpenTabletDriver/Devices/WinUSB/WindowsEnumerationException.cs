using System;

namespace OpenTabletDriver.Devices.WinUSB
{
    public class WindowsEnumerationException : Exception
    {
        public WindowsEnumerationException(string msg)
            : base(msg)
        {
        }
    }
}
