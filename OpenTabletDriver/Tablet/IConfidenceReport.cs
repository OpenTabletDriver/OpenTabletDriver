using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// A report containing confidence information.
    /// </summary>
    [PublicAPI]
    public interface IConfidenceReport : IDeviceReport
    {
        /// <summary>
        /// Whether the data has high confidence of being accurate.
        /// </summary>
        bool HighConfidence { set; get; }
    }
}
