using Newtonsoft.Json;
using OpenTabletDriver.Platform.Display;

namespace OpenTabletDriver.Daemon.Contracts
{
    public class DisplayDto
    {
        [JsonConstructor]
        public DisplayDto(
            int index,
            float width,
            float height,
            float x,
            float y
        )
        {
            Index = index;
            Width = width;
            Height = height;
            X = x;
            Y = y;
        }

        public DisplayDto(IDisplay display)
        {
            Index = display.Index;
            Width = display.Width;
            Height = display.Height;
            X = display.Position.X;
            Y = display.Position.Y;
        }

        public int Index { get; }
        public float Width { get; }
        public float Height { get; }
        public float X { get; }
        public float Y { get; }
    }
}
