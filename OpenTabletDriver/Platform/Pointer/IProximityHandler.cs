using JetBrains.Annotations;

namespace OpenTabletDriver.Platform.Pointer
{
    /// <summary>
    /// A pen proximity handler.
    /// </summary>
    [PublicAPI]
    public interface IProximityHandler
    {
        /// <summary>
        /// Sets the pen proximity state.
        /// </summary>
        /// <param name="proximity">
        /// Whether the pen is in proximity to the tablet.
        /// </param>
        void SetProximity(bool proximity);
    }
}
