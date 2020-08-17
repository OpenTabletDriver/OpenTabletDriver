namespace TabletDriverPlugin.Platform.Pointer
{
    public interface IVirtualTablet : IVirtualPointer
    {
        void SetPosition(Point pos);
    }
}