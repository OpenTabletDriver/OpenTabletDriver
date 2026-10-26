namespace OpenTabletDriver.Plugin.Output
{
    public interface IPositionedPipelineElement : IPipelineElement
    {
        /// <summary>
        /// The position in which this <see cref="IPipelineElement"/> will be processed.
        /// This helps determine what the expected input units will be.
        /// </summary>
        PipelinePosition Position { get; }
    }
}
