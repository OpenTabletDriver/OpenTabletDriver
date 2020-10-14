using System;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Interop
{
    [PluginIgnore]
    public abstract class EvdevVirtualPointer : IVirtualPointer, IDisposable
    {
        protected EvdevDevice Device { set; get; }

        public void MouseDown(MouseButton button)
        {
            if (GetCode(button) is EventCode code)
            {
                Device.Write(EventType.EV_KEY, code, 1);
                Device.Sync();
            }
        }

        public void MouseUp(MouseButton button)
        {
            if (GetCode(button) is EventCode code)
            {
                Device.Write(EventType.EV_KEY, code, 0);
                Device.Sync();
            }
        }

        protected virtual EventCode? GetCode(MouseButton button) => button switch
        {
            MouseButton.Left     => EventCode.BTN_LEFT,
            MouseButton.Middle   => EventCode.BTN_MIDDLE,
            MouseButton.Right    => EventCode.BTN_RIGHT,
            MouseButton.Forward  => EventCode.BTN_FORWARD,
            MouseButton.Backward => EventCode.BTN_BACK,
            _                    => null
        };

        public virtual void Dispose()
        {
            Device?.Dispose();
        }
    }
}