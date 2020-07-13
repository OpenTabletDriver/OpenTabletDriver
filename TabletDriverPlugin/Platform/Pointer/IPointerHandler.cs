namespace TabletDriverPlugin.Platform.Pointer
{
    public interface IPointerHandler
    {
        Point GetPosition();
        void SetPosition(Point pos);
    }
}