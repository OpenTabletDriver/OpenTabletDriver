using System;
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

        public float Width { set; get; } = 0;
        public float Height { set; get; } = 0;
        public Vector2 Position { set; get; } = new Vector2();
        public float Rotation { set; get; } = 0;

        public float[] GetRotationMatrix()
        {
            var angle = -Rotation * (Math.PI / 180);
            return new float[4]
            {
                (float)Math.Cos(angle),
                (float)-Math.Sin(angle),
                (float)Math.Sin(angle),
                (float)Math.Cos(angle)
            };
        }

        public override string ToString() => $"[{Width}x{Height}@{Position}:{Rotation}°],";
        public static implicit operator string(Area area) => area.ToString();
    }
}