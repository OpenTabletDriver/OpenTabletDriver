using JetBrains.Annotations;

namespace OpenTabletDriver.Platform.Pointer
{
    /// <summary>
    /// A pen pressure handler.
    /// </summary>
    [PublicAPI]
    public interface IPressureHandler : IAbsolutePointer
    {
        /// <summary>
        /// Sets the pressure of the pen.
        /// </summary>
        /// <param name="percentage">
        /// The percentage of pressure output, ranging from 0 to 100.
        /// </param>
        void SetPressure(float percentage);
    }
}
