using TabletDriverPlugin;

namespace TabletDriverPlugin.Platform.Display
{
    public interface IDisplay
    {
        int Index { get; }
        float Width { get; }
        float Height { get; }
        Point Position { get; }
    }
}