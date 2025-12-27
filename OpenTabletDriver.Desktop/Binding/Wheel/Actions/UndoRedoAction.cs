using OpenTabletDriver.Plugin.Tablet.Wheel.Actions;
using OpenTabletDriver.Desktop.Interop;

namespace OpenTabletDriver.Desktop.Binding.Wheel.Actions
{
    public class UndoRedoAction : IWheelAction
    {
        public void Execute(int direction)
        {
            var kb = DesktopInterop.VirtualKeyboard;

            if (direction > 0)
            {
                kb.Press("LeftControl");
                kb.Press("LeftShift");
                kb.Press("Z");
                kb.Release("Z");
                kb.Release("LeftShift");
                kb.Release("LeftControl");
            }
            else
            {
                kb.Press("LeftControl");
                kb.Press("Z");
                kb.Release("Z");
                kb.Release("LeftControl");
            }
        }
    }
}
