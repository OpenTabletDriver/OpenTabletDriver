using System.Numerics;
using OpenTabletDriver.Platform.Display;

namespace OpenTabletDriver.Daemon.Interop.Display
{
    internal class Display : IDisplay
    {
        internal Display(float width, float height, Vector2 position, int index = 0)
        {
            Width = width;
            Height = height;
            Position = position;
            Index = index;
        }

        public int Index { private set; get; }
        public float Width { private set; get; }
        public float Height { private set; get; }
        public Vector2 Position { private set; get; }

        public override string ToString()
        {
            return $"Display {Index} ({Width}x{Height}@{Position})";
        }
    }
}
