using JetBrains.Annotations;

namespace OpenTabletDriver.Platform.Pointer
{
    /// <summary>
    /// A pen confidence handler. Used to help output modes correctly report tool reports.
    /// </summary>
    [PublicAPI]
    public interface IConfidenceHandler
    {
        /// <summary>
        /// Sets the active pen confidence flag
        /// </summary>
        /// <param name="highConfidence">
        /// Sets the flag for whether the pen is confidently in range, and all parameters can likely be received correctly.
        /// This is defined by the tablet parser, or tablet firmware.
        /// </param>
        void SetConfidence(bool highConfidence);
    }
}
