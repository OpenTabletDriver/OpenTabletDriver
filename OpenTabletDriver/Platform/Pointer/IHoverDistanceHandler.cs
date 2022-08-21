using JetBrains.Annotations;

namespace OpenTabletDriver.Platform.Pointer
{
    /// <summary>
    /// A pen hover distance handler.
    /// </summary>
    [PublicAPI]
    public interface IHoverDistanceHandler
    {
        /// <summary>
        /// Sets the active pen hover distance.
        /// </summary>
        /// <param name="distance">
        /// Sets the pen hover distance. This is an arbitrary value with no defined real-world units.
        /// </param>
        void SetHoverDistance(uint distance);
    }
}
