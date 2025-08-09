using System;

namespace OpenTabletDriver.Plugin.Output
{
    public interface IPipelineElement<T>
    {
        /// <summary>
        /// Handles an object to be processed by the <see cref="IPipelineElement{T}"/>.
        /// </summary>
        void Consume(T value);

        /// <summary>
        /// Invoked when an object of <see cref="T"/> is to be pushed to the next link in the pipeline.
        /// </summary>
        event Action<T> Emit;
    }
}
