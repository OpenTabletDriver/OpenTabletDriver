using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using OpenTabletDriver.Plugin.Platform.Display;

namespace OpenTabletDriver.Desktop.Interop.Display
{
    internal class Display : IDisplay
    {
        [SetsRequiredMembers]
        internal Display(float width, float height, Vector2 position, int index = 0)
        {
            Width = width;
            Height = height;
            Position = position;
            Index = index;
        }

        public required int Index { internal init; get; }
        public required float Width { internal init; get; }
        public required float Height { internal init; get; }
        public required Vector2 Position { internal init; get; }

        public override string ToString()
        {
            return $"Display {Index} ({Width}x{Height}@{Position})";
        }
    }
}
