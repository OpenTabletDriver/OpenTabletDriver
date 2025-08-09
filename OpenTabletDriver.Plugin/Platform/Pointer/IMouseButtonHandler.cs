namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IMouseButtonHandler
    {
        void MouseDown(MouseButton button);
        void MouseUp(MouseButton button);
    }
}
