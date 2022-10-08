using System.ComponentModel;
using System.Linq;
using System.Numerics;
using JetBrains.Annotations;

namespace OpenTabletDriver
{
    /// <summary>
    /// A working area designating width and height based at a centered origin position.
    /// </summary>
    [PublicAPI]
    public class Area : NotifyPropertyChanged
    {
        private float _width, _height, _xPosition, _yPosition;

        /// <summary>
        /// The width of the area.
        /// </summary>
        [DisplayName(nameof(Width))]
        public float Width
        {
            set => RaiseAndSetIfChanged(ref _width, value);
            get => _width;
        }

        /// <summary>
        /// The height of the area.
        /// </summary>
        [DisplayName(nameof(Height))]
        public float Height
        {
            set => RaiseAndSetIfChanged(ref _height, value);
            get => _height;
        }

        /// <summary>
        /// The X component of the area's center offset.
        /// </summary>
        [DisplayName("X")]
        public float XPosition
        {
            set => RaiseAndSetIfChanged(ref _xPosition, value);
            get => _xPosition;
        }

        /// <summary>
        /// The Y component of the area's center offset.
        /// </summary>
        [DisplayName("Y")]
        public float YPosition
        {
            set => RaiseAndSetIfChanged(ref _yPosition, value);
            get => _yPosition;
        }

        /// <summary>
        /// Returns the center offset of the area.
        /// </summary>
        /// <remarks>
        /// This is also the rotation origin of the area where applicable.
        /// </remarks>
        public Vector2 GetPosition() => new Vector2(XPosition, YPosition);

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

        /// <summary>
        /// Returns the center offset of the area.
        /// </summary>
        public virtual Vector2 GetCenterOffset()
        {
            var corners = GetCorners();
            var max = new Vector2(
                corners.Max(v => v.X),
                corners.Max(v => v.Y)
            );
            var min = new Vector2(
                corners.Min(v => v.X),
                corners.Min(v => v.Y)
            );
            return (max - min) / 2;
        }

        public override string ToString() => $"[{Width}x{Height}@{GetPosition()}]";
    }
}
