namespace TabletDriverLib.Interop.Keyboard
{
    public interface IKeyboardHandler
    {
        void Press(string key);
        void Release(string key);
    }
}