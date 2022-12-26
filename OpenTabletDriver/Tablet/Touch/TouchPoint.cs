using System.Numerics;
using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet.Touch
{
    /// <summary>
    /// A touched point from an <see cref="ITouchReport"/>.
    /// </summary>
    [PublicAPI]
    public struct TouchPoint
    {
        /// <summary>
        /// The identifier of the touch point.
        /// </summary>
        public byte TouchID { init; get; }

        /// <summary>
        /// The absolute position in which the touch point was sourced.
        /// </summary>
        public Vector2 Position { init; get; }

        public override string ToString()
        {
            return $"Point #{TouchID}: {Position};";
        }
    }
}
