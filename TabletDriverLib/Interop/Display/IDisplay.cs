using TabletDriverLib.Component;

namespace TabletDriverLib.Interop.Display
{
    public interface IDisplay
    {
        int Index { get; }
        float Width { get; }
        float Height { get; }
        Point Position { get; }
    }
}