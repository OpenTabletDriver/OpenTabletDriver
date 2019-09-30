using System.Runtime.InteropServices;

namespace TabletDriverLib.Tools.Native
{
    internal class Windows
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);
    }
}