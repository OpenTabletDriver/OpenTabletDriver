using System;
using System.Linq;
using System.Numerics;
using JetBrains.Annotations;

namespace OpenTabletDriver
{
    /// <summary>
    /// An <see cref="Area"/> supporting rotation.
    /// </summary>
    [PublicAPI]
    public class AngledArea : Area
    {
        /// <summary>
        /// The rotation angle of the area.
        /// </summary>
        public float Rotation { get; set; }

        public override Vector2[] GetCorners()
        {
            var origin = GetPosition();
            var matrix = Matrix3x2.CreateTranslation(-origin);
            matrix *= Matrix3x2.CreateRotation((float)(Rotation * Math.PI / 180));
            matrix *= Matrix3x2.CreateTranslation(origin);

            return base.GetCorners().Select(c => Vector2.Transform(c, matrix)).ToArray();
        }

        public override string ToString() => $"[{Width}x{Height}@{GetPosition()}:{Rotation}°]";
    }
}
