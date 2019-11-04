using NativeLib.Linux;
using TabletDriverLib.Interop.Input;

namespace TabletDriverLib.Interop.Converters
{
    internal class XButtonConverter : IConverter<MouseButton, XMouseButton>
    {
        public MouseButton Convert(XMouseButton obj)
        {
            switch (obj)
            {
                case XMouseButton.LEFT:
                    return MouseButton.Left;
                case XMouseButton.MIDDLE:
                    return MouseButton.Middle;
                case XMouseButton.RIGHT:
                    return MouseButton.Right;
                default:
                    return 0;
            }
        }

        public XMouseButton Convert(MouseButton obj)
        {
            switch (obj)
            {
                case MouseButton.Left:
                    return XMouseButton.LEFT;
                case MouseButton.Middle:
                    return XMouseButton.MIDDLE;
                case MouseButton.Right:
                    return XMouseButton.RIGHT;
                case MouseButton.Forward:
                    return XMouseButton.FORWARD;
                case MouseButton.Backward:
                    return XMouseButton.BACKWARD;
                default:
                    return 0;
            }
        }
    }
}