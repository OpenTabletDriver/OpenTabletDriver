using System.Numerics;
using JetBrains.Annotations;

namespace OpenTabletDriver
{
    /// <summary>
    /// A working area designating width and height based at a centered origin position.
    /// </summary>
    [PublicAPI]
    public class Area
    {
        /// <summary>
        /// The width of the area.
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// The height of the area.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// The X component of the area's center offset.
        /// </summary>
        public float XPosition { get; set; }

        /// <summary>
        /// The Y component of the area's center offset.
        /// </summary>
        public float YPosition { get; set; }

        /// <summary>
        /// Returns the center offset of the area.
        /// </summary>
        /// <remarks>
        /// This is also the rotation origin of the area where applicable.
        /// </remarks>
        public Vector2 GetPosition() => new(XPosition, YPosition);

        /// <summary>
        /// Returns all corners of the area.
        /// </summary>
        public virtual Vector2[] GetCorners()
        {
            var halfWidth = Width / 2;
            var halfHeight = Height / 2;

            var x = XPosition;
            var y = YPosition;

            return new[]
            {
                new Vector2(x - halfWidth, y - halfHeight),
                new Vector2(x - halfWidth, y + halfHeight),
                new Vector2(x + halfWidth, y + halfHeight),
                new Vector2(x + halfWidth, y - halfHeight)
            };
        }

        public override string ToString() => $"[{Width}x{Height}@{GetPosition()}]";
    }
}
