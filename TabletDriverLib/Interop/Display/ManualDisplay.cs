using TabletDriverLib.Component;

namespace TabletDriverLib.Interop.Display
{
    internal class ManualDisplay : IDisplay
    {
        internal ManualDisplay(float width, float height, Point position)
        {
            Width = width;
            Height = height;
            Position = position;
        }

        public float Width { private set; get; }
        public float Height { private set; get; }
        public Point Position { private set; get; }
    }
}