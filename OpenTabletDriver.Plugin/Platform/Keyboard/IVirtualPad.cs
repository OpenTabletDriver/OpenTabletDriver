using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Platform.Keyboard
{
    public interface IVirtualPad
    {
        void KeyEvent(TabletPadEvent padEvent, bool isPress);
    }
}
