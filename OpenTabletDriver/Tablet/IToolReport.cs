using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// A device report containing information about the current tool.
    /// </summary>
    [PublicAPI]
    public interface IToolReport : IDeviceReport
    {
        /// <summary>
        /// A serial number for the currently active tool.
        /// </summary>
        ulong Serial { set; get; }

        /// <summary>
        /// The currently active tool identifier, typically used like a model number.
        /// </summary>
        uint RawToolID { set; get; }

        /// <summary>
        /// The currently active tool type.
        /// </summary>
        ToolType Tool { set; get; }
    }
}
