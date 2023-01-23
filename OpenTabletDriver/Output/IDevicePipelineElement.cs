using OpenTabletDriver.Attributes;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Output
{
    /// <summary>
    /// A pipeline element with a predefined position within the pipeline.
    /// </summary>
    /// <typeparam name="T">
    /// The pipeline element type.
    /// </typeparam>
    [PluginInterface]
    public interface IDevicePipelineElement : IPipelineElement<IDeviceReport>
    {
        /// <summary>
        /// The position in which this <see cref="IPipelineElement{T}"/> will be processed.
        /// This helps determine what the expected input units will be.
        /// </summary>
        PipelinePosition Position { get; }
    }
}
