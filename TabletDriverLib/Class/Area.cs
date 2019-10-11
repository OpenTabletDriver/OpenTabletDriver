namespace TabletDriverLib.Class
{
    public class Area
    {
        public Area()
        {
        }

        public Area(float width, float height, Point position, float rotation)
        {
            Width = width;
            Height = height;
            Position = position;
            Rotation = rotation;
        }

        public float Width { set; get; } = 0;
        public float Height { set; get; } = 0;
        public Point Position { set; get; } = new Point();

        public float Rotation { set; get; } = 0;

        public override string ToString() => $"[{Width}x{Height}@{Position}:{Rotation}°],";
        public static implicit operator string(Area area) => area.ToString();
    }
}