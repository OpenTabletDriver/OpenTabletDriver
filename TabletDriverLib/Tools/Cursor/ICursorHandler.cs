using TabletDriverLib.Class;

namespace TabletDriverLib.Tools.Cursor
{
    public interface ICursorHandler
    {
        Point GetCursorPosition();
        void SetCursorPosition(Point pos);
    }
}