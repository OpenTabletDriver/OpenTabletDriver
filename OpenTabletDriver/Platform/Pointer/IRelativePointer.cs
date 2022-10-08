using System.Numerics;
using JetBrains.Annotations;

namespace OpenTabletDriver.Platform.Pointer
{
    /// <summary>
    /// A relative positioned pointer. Accepts a relatively positioned vector and invokes a handler performing relative
    /// positioning.
    /// </summary>
    [PublicAPI]
    public interface IRelativePointer : IMouseButtonHandler
    {
        /// <summary>
        /// Sets a relative position.
        /// </summary>
        /// <param name="delta">
        /// The position to change the output by in pixels.
        /// </param>
        void SetPosition(Vector2 delta);
    }
}
