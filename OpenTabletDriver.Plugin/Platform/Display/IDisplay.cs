using System.Numerics;

namespace OpenTabletDriver.Plugin.Platform.Display
{
    public interface IDisplay
    {
        int Index { get; }
        float Width { get; }
        float Height { get; }
        Vector2 Position { get; }
    }
}
