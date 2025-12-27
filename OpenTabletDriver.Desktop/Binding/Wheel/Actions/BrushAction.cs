using OpenTabletDriver.Plugin.Tablet.Wheel.Actions;
using OpenTabletDriver.Desktop.Interop;

namespace OpenTabletDriver.Desktop.Binding.Wheel.Actions
{
    public class BrushAction : IWheelAction
    {
        public void Execute(int direction)
        {
            var kb = DesktopInterop.VirtualKeyboard;

            if (direction > 0)
            {
                kb.Press("BracketRight");
                kb.Release("BracketRight");
            }
            else
            {
                kb.Press("BracketLeft");
                kb.Release("BracketLeft");
            }
        }
    }
}
