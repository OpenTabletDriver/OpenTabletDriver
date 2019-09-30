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

        public float Width { set; get; }
        public float Height { set; get; }
        public Point Position { set; get; }

        public float Rotation { set; get; }
    }
}