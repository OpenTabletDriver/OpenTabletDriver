using System.Numerics;

namespace OpenTabletDriver.Plugin
{
    public class Area
    {
        public Area()
        {
        }

        public Area(float width, float height, Vector2 position, float rotation)
        {
            Width = width;
            Height = height;
            Position = position;
            Rotation = rotation;
        }

        /// <summary>
        /// The width of the area.
        /// </summary>
        public float Width { set; get; } = 0;

        /// <summary>
        /// The height of the area.
        /// </summary>
        public float Height { set; get; } = 0;

        /// <summary>
        /// The center offset of the area.
        /// </summary>
        /// <remarks>
        /// This is also the rotation point of the area.
        /// </remarks>
        public Vector2 Position { set; get; } = new Vector2();

        /// <summary>
        /// The rotation angle of the area.
        /// </summary>
        public float Rotation { set; get; } = 0;

        public override string ToString() => $"[{Width}x{Height}@{Position}:{Rotation}°],";
    }
}
