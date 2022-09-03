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
    }
}
