using System.Collections.Generic;

namespace OpenTabletDriver.Plugin.Platform.Keyboard
{
    public interface IVirtualPad
    {
        void KeyEvent(string key, bool isPress);
    }
}
