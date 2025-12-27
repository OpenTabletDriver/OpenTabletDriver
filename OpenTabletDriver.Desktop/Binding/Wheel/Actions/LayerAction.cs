using OpenTabletDriver.Plugin.Tablet.Wheel.Actions;
using OpenTabletDriver.Desktop.Interop;

namespace OpenTabletDriver.Desktop.Binding.Wheel.Actions
{
    public class LayerAction : IWheelAction
    {
        public void Execute(int direction)
        {
            var kb = DesktopInterop.VirtualKeyboard;

            string key = direction > 0 ? "PageUp" : "PageDown";
            kb.Press(key);
            kb.Release(key);
        }
    }
}
