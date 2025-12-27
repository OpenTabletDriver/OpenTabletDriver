using OpenTabletDriver.Plugin.Tablet.Wheel.Actions;
using OpenTabletDriver.Desktop.Interop;

namespace OpenTabletDriver.Desktop.Binding.Wheel.Actions
{
    public class ZoomAction : IWheelAction
    {
        public void Execute(int direction)
        {
            var kb = DesktopInterop.VirtualKeyboard;

            kb.Press("LeftControl");

            if (direction > 0)
            {
                kb.Press("Equal");
                kb.Release("Equal");
            }
            else
            {
                kb.Press("Minus");
                kb.Release("Minus");
            }

            kb.Release("LeftControl");
        }
    }
}
