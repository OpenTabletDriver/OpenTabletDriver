using System;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop
{
    [PluginIgnore]
    public abstract class EvdevVirtualMouse : IMouseButtonHandler, ISynchronousPointer, IDisposable
    {
        protected EvdevDevice Device { set; get; }

        public bool HasValidDevice() => Device?.CanWrite ?? false;

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
            Device = null;
        }

        public void Flush()
        {
            Device?.Sync();
        }

        public virtual void Reset()
        {
        }
    }
}
