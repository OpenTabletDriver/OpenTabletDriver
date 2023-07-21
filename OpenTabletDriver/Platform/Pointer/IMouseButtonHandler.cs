using System.Numerics;
using JetBrains.Annotations;

namespace OpenTabletDriver.Platform.Pointer
{
    /// <summary>
    /// A mouse button handler.
    /// </summary>
    [PublicAPI]
    public interface IMouseButtonHandler
    {
        /// <summary>
        /// Presses a mouse button.
        /// </summary>
        /// <param name="button">
        /// The button to press.
        /// </param>
        void MouseDown(MouseButton button);

        /// <summary>
        /// Releases a mouse button.
        /// </summary>
        /// <param name="button">
        /// The button to release.
        /// </param>
        void MouseUp(MouseButton button);

        /// <summary>
        /// Invokes a mouse scroll event.
        /// </summary>
        /// <param name="delta">
        /// The relative position change.
        /// </param>
        void Scroll(Vector2 delta);
    }
}
