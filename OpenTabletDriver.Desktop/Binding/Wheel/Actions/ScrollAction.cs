using OpenTabletDriver.Plugin.Tablet.Wheel.Actions;
using OpenTabletDriver.Desktop.Interop;

namespace OpenTabletDriver.Desktop.Binding.Wheel.Actions
{
    public class ScrollAction : IWheelAction
    {
        public void Execute(int direction)
        {
            var kb = DesktopInterop.VirtualKeyboard;

            if (direction > 0)
            {
                kb.Press("Up");
                kb.Release("Up");
            }
            else
            {
                kb.Press("Down");
                kb.Release("Down");
            }
        }
    }
}
