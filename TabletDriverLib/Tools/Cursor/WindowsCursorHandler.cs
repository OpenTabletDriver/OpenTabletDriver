using TabletDriverLib.Class;

namespace TabletDriverLib.Tools.Cursor
{
    public class WindowsCursorHandler : ICursorHandler
    {
        public void SetCursorPosition(Point pos)
        {
            Native.Windows.SetCursorPos((int)pos.X, (int)pos.Y);
        }
    }
}