using TabletDriverPlugin;

namespace TabletDriverPlugin.Platform.Pointer
{
    public interface ICursorHandler
    {
        Point GetCursorPosition();
        void SetCursorPosition(Point pos);

        void MouseDown(MouseButton button);
        void MouseUp(MouseButton button);
    }
}