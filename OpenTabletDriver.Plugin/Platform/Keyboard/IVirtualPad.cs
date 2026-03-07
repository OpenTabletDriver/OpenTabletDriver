using System.ComponentModel.DataAnnotations;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Platform.Keyboard
{
    public interface IVirtualPad
    {
        void KeyEvent(TabletPadEvent padEvent, bool isPress);

        void WheelEvent([Range(0, 359)] uint? degrees);
    }
}
