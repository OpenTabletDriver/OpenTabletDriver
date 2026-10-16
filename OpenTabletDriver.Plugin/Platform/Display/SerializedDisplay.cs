using System.Numerics;
using Newtonsoft.Json;

namespace OpenTabletDriver.Plugin.Platform.Display
{
    [method: JsonConstructor]
    public record SerializedDisplay(
        float Width,
        float Height,
        Vector2 Position,
        string? Name = "",
        bool IsVirtualScreen = false,
        int Index = 0)
        : IDisplay
    {
        public SerializedDisplay(IDisplay display)
            : this(
                Width: display.Width,
                Height: display.Height,
                Position: display.Position,
                IsVirtualScreen: display is IVirtualScreen,
                Index: display.Index
                )
        { }

        public override string ToString()
        {
            string header = IsVirtualScreen
                ? string.IsNullOrEmpty(Name) ? "Virtual Screen" : Name
                : $"Display {Index}{(string.IsNullOrEmpty(Name) ? string.Empty : $" \"{Name}\"")}";

            return $"{header} ({Width}x{Height}@{Position})";
        }
    }
}
