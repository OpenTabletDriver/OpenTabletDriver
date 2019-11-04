using TabletDriverLib.Component;

namespace TabletDriverLib.Interop.Input
{
    public interface IInputHandler
    {
        Point GetCursorPosition();
        void SetCursorPosition(Point pos);

        void MouseDown(MouseButton button);
        void MouseUp(MouseButton button);
        bool GetMouseButtonState(MouseButton button);
    }
}