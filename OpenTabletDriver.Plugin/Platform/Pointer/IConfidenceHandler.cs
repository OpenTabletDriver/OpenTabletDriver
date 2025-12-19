namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IConfidenceHandler
    {
        /// <summary>
        /// Sets whether the reports are confident or not. This is usually determined by <see cref="Tablet.IProximityReport.NearProximity"/>.
        /// This should be set for every report, but output modes might save its state.
        ///
        /// However, if the output mode also implements <see cref="ISynchronousPointer.Reset()"/>, it should always remove confidence
        /// </summary>
        /// <param name="isConfident">Report is confident</param>
        void SetConfidence(bool isConfident);

        /// <summary>
        /// Enable or disable confidence handling entirely.
        ///
        /// Useful if the user is not interested in the side effects caused by proximity handling (such as reduced lift-off distance)
        /// </summary>
        /// <param name="enableConfidenceHandling"></param>
        void SetConfidenceHandling(bool enableConfidenceHandling);
    }
}
