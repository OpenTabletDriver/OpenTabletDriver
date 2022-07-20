using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Output
{
    /// <summary>
    /// A pipeline element in which <see cref="T"/> is modified.
    /// </summary>
    /// <typeparam name="T">
    /// The pipeline element type.
    /// </typeparam>
    [PublicAPI]
    public interface IPipelineElement<T>
    {
        /// <summary>
        /// Handles an object to be processed by the <see cref="IPipelineElement{T}"/>.
        /// </summary>
        void Consume(T value);

        /// <summary>
        /// Invoked when an object of <see cref="T"/> is to be pushed to the next link in the pipeline.
        /// </summary>
        event Action<T>? Emit;
    }
}
