using JetBrains.Annotations;

namespace OpenTabletDriver.Platform.Pointer
{
    /// <summary>
    /// A native eraser handler, commonly used in drawing applications to apply an erase function or binding.
    /// </summary>
    [PublicAPI]
    public interface IEraserHandler
    {
        /// <summary>
        /// Sets the current eraser state.
        /// </summary>
        /// <param name="isEraser">
        /// Whether the eraser is the active tip.
        /// </param>
        void SetEraser(bool isEraser);
    }
}
