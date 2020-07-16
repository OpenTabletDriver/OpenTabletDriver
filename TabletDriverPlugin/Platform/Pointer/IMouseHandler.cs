namespace TabletDriverPlugin.Platform.Pointer
{
    public interface IMouseHandler : IPointerHandler
    {
        void MouseDown(MouseButton button);
        void MouseUp(MouseButton button);
    }
}