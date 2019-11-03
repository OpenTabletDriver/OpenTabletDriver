using TabletDriverLib.Component;

namespace TabletDriverLib.Interop.Cursor
{
    public interface ICursorHandler
    {
        Point GetCursorPosition();
        void SetCursorPosition(Point pos);

        void MouseDown(MouseButton button);
        void MouseUp(MouseButton button);
        bool GetMouseButtonState(MouseButton button);
    }
}