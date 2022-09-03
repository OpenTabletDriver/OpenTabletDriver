using System.Numerics;
using JetBrains.Annotations;

namespace OpenTabletDriver.Platform.Display
{
    /// <summary>
    /// A physical display from the display server.
    /// </summary>
    [PublicAPI]
    public interface IDisplay
    {
        /// <summary>
        /// The index of the display, typically in order top left to bottom right.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// The width of the display in pixels.
        /// </summary>
        float Width { get; }

        /// <summary>
        /// The height of the display in pixels.
        /// </summary>
        float Height { get; }

        /// <summary>
        /// The offset of the display in pixels, anchored from the top left corner of the display.
        /// </summary>
        Vector2 Position { get; }
    }
}
