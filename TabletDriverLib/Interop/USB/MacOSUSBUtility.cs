using System;
using NativeLib.OSX;

namespace TabletDriverLib.Interop.USB
{
    using static NativeLib.OSX.OSX;

    public class MacoSUSBUtility : IUSBUtility
    {
        public bool InitStrings(string hidDvicePath, byte[] array)
        {
            return true;
        }
    }
}