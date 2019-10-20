using TabletDriverLib.Component;

namespace TabletDriverLib.Interop.Cursor
{
    public class WindowsCursorHandler : ICursorHandler
    {
        public Point GetCursorPosition()
        {
            Native.Windows.GetCursorPos(out Native.Windows.POINT pt);
            return (Point)pt;
        }

        public void SetCursorPosition(Point pos)
        {
            Native.Windows.SetCursorPos((int)pos.X, (int)pos.Y);
        }
    }
}