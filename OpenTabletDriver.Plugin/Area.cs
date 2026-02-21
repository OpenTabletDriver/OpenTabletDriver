using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace OpenTabletDriver.Plugin
{
    [method: SetsRequiredMembers]
    public class Area(float width, float height, Vector2 position = default, float rotation = 0)
    {
        /// <summary>
        /// The width of the area.
        /// </summary>
        public required float Width { set; get; } = width;

        /// <summary>
        /// The height of the area.
        /// </summary>
        public required float Height { set; get; } = height;

        /// <summary>
        /// The center offset of the area.
        /// </summary>
        /// <remarks>
        /// This is also the rotation point of the area.
        /// </remarks>
        public Vector2 Position { set; get; } = position;

        /// <summary>
        /// The rotation angle of the area.
        /// </summary>
        public float Rotation { set; get; } = rotation;

        public override string ToString() => $"[{Width}x{Height}@{Position}:{Rotation}°],";
    }
}
