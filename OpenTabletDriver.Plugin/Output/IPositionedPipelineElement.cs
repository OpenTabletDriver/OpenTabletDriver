namespace OpenTabletDriver.Plugin.Output
{
    public interface IPositionedPipelineElement<T> : IPipelineElement<T>
    {
        /// <summary>
        /// The position in which this <see cref="IPipelineElement{T}"/> will be processed.
        /// This helps determine what the expected input units will be.
        /// </summary>
        PipelinePosition Position { get; }
    }
}
