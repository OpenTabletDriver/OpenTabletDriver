using TabletDriverLib.Class;

namespace TabletDriverLib.Tools.Cursor
{
    public class WindowsCursorHandler : ICursorHandler
    {
        public Point GetCursorPosition()
        {
            Native.Windows.GetCursorPos(out Native.Windows.POINT pt);
            return new Point(pt.X, pt.Y);
        }

        public void SetCursorPosition(Point pos)
        {
            Native.Windows.SetCursorPos((int)pos.X, (int)pos.Y);
        }
    }
}