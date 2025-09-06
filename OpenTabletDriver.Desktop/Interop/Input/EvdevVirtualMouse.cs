using System;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop
{
    [PluginIgnore]
    public abstract class EvdevVirtualMouse : IMouseButtonHandler, IMouseScrollHandler, ISynchronousPointer, IDisposable
    {
        protected EvdevDevice Device { set; get; }

        public void MouseDown(MouseButton button)
        {
            if (GetCode(button) is EventCode code)
            {
                Device.Write(EventType.EV_KEY, code, 1);
            }
        }

        public void MouseUp(MouseButton button)
        {
            if (GetCode(button) is EventCode code)
            {
                Device.Write(EventType.EV_KEY, code, 0);
            }
        }

        public void ScrollVertically(int amount)
        {
            Device.Write(EventType.EV_REL, EventCode.REL_WHEEL_HI_RES, amount);
        }

        public void ScrollHorizontally(int amount)
        {
            Device.Write(EventType.EV_REL, EventCode.REL_HWHEEL_HI_RES, amount);
        }

        protected virtual EventCode? GetCode(MouseButton button) => button switch
        {
            MouseButton.Left => EventCode.BTN_LEFT,
            MouseButton.Middle => EventCode.BTN_MIDDLE,
            MouseButton.Right => EventCode.BTN_RIGHT,
            MouseButton.Backward => EventCode.BTN_SIDE,
            MouseButton.Forward => EventCode.BTN_EXTRA,
            _ => null
        };

        public virtual void Dispose()
        {
            Device?.Dispose();
        }

        public void Flush()
        {
            Device.Sync();
        }

        public virtual void Reset()
        {
        }
    }
}
