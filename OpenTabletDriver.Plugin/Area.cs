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
        public required float Width { get; set; } = width;

        /// <summary>
        /// The height of the area.
        /// </summary>
        public required float Height { get; set; } = height;

        /// <summary>
        /// The center offset of the area.
        /// </summary>
        /// <remarks>
        /// This is also the rotation point of the area.
        /// </remarks>
        public Vector2 Position { get; set; } = position;

        /// <summary>
        /// The rotation angle of the area.
        /// </summary>
        public float Rotation { get; set; } = rotation;

        public override string ToString() => $"[{Width}x{Height}@{Position}:{Rotation}°],";
    }
}
