using TabletDriverPlugin;

namespace TabletDriverLib.Interop.Display
{
    internal class ManualDisplay : IDisplay
    {
        internal ManualDisplay(float width, float height, Point position, int index = 0)
        {
            Width = width;
            Height = height;
            Position = position;
            Index = index;
        }

        public int Index { private set; get; }
        public float Width { private set; get; }
        public float Height { private set; get; }
        public Point Position { private set; get; }

        public override string ToString()
        {
            return $"Display {Index} ({Width}x{Height}@{Position})";
        }
    }
}