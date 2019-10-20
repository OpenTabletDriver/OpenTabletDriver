using TabletDriverLib.Component;

namespace TabletDriverLib.Interop.Cursor
{
    public interface ICursorHandler
    {
        Point GetCursorPosition();
        void SetCursorPosition(Point pos);
    }
}