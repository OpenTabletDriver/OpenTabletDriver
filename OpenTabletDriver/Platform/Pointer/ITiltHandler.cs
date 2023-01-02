using System.Numerics;

namespace OpenTabletDriver.Platform.Pointer
{
    /// <summary>
    /// A pen tilt handler.
    /// </summary>
    public interface ITiltHandler
    {
        /// <summary>
        /// Sets the pen tilt angle.
        /// </summary>
        /// <param name="tilt">
        /// The current tilt angle. This is an arbitrary value with no defined real-world units.
        /// </param>
        void SetTilt(Vector2 tilt);
    }
}
