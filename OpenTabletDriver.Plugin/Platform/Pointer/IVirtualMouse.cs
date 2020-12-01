namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IVirtualMouse
    {
        void MouseDown(MouseButton button);
        void MouseUp(MouseButton button);
    }
}
