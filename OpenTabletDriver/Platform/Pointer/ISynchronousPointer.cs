using JetBrains.Annotations;

namespace OpenTabletDriver.Platform.Pointer
{
    /// <summary>
    /// A pointer which requires syncing to update the current state.
    /// </summary>
    [PublicAPI]
    public interface ISynchronousPointer
    {
        /// <summary>
        /// Resets the pointer's state.
        /// </summary>
        void Reset();

        /// <summary>
        /// Flushes the current state to the native handler.
        /// </summary>
        void Flush();
    }
}
