using TabletDriverLib.Component;

namespace TabletDriverLib.Interop.Display
{
    public interface IDisplay
    {
        float Width { get; }
        float Height { get; }
        Point Position { get; }
    }
}