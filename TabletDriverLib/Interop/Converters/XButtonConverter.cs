using TabletDriverLib.Interop.Cursor;
using static TabletDriverLib.Interop.Native.Linux;

namespace TabletDriverLib.Interop.Converters
{
    internal class XButtonConverter : IConverter<MouseButton, Button>
    {
        public MouseButton Convert(Button obj)
        {
            switch (obj)
            {
                case Button.LEFT:
                    return MouseButton.Left;
                case Button.MIDDLE:
                    return MouseButton.Middle;
                case Button.RIGHT:
                    return MouseButton.Right;
                default:
                    return 0;
            }
        }

        public Button Convert(MouseButton obj)
        {
            switch (obj)
            {
                case MouseButton.Left:
                    return Button.LEFT;
                case MouseButton.Middle:
                    return Button.MIDDLE;
                case MouseButton.Right:
                    return Button.RIGHT;
                case MouseButton.Forward:
                    return Button.FORWARD;
                case MouseButton.Backward:
                    return Button.BACKWARD;
                default:
                    return 0;
            }
        }
    }
}