using System;
using System.Collections.Generic;
using OpenTabletDriver.Native.Windows.Input;
using OpenTabletDriver.Platform.Keyboard;

namespace OpenTabletDriver.Daemon.Interop.Input.Keyboard
{
    public class WindowsKeysProvider : IKeyMapper
    {
        public object this[BindableKey key] => (VirtualKey)key;

        public IEnumerable<BindableKey> GetBindableKeys()
        {
            return Enum.GetValues<BindableKey>();
        }
    }
}
