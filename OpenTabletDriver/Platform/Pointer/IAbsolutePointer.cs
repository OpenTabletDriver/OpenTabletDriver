using System.Numerics;
using JetBrains.Annotations;

namespace OpenTabletDriver.Platform.Pointer
{
    /// <summary>
    /// An absolute positioned pointer. Accepts an absolutely positioned vector and invokes a handler performing
    /// absolute positioning.
    /// </summary>
    [PublicAPI]
    public interface IAbsolutePointer : IMouseButtonHandler
    {
        /// <summary>
        /// Sets an absolute position.
        /// </summary>
        /// <param name="pos">
        /// The position to set mapped to the output in pixels.
        /// </param>
        void SetPosition(Vector2 pos);
    }
}
