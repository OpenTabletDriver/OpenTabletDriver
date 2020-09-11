namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IVirtualMouse : IVirtualPointer
    {
        void Move(float dX, float dY);
    }
}