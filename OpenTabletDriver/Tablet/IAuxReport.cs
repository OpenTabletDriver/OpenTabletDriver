using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// An auxiliary report containing states of express keys.
    /// </summary>
    [PublicAPI]
    public interface IAuxReport : IDeviceReport
    {
        /// <summary>
        /// The states of all express keys.
        /// </summary>
        bool[] AuxButtons { set; get; }
    }
}
