
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Output
{
    public delegate void RefAction<T>(ref T item) where T : struct, allows ref struct;

    public interface IPipelineElement
    {
        /// <summary>
        /// Handles an object to be processed by the <see cref="IPipelineElement"/>.
        /// </summary>
        void Consume(scoped ref DeviceReport obj);

        /// <summary>
        /// Invoked when a DeviceReport is to be pushed to the next link in the pipeline.
        /// </summary>
        event RefAction<DeviceReport>? Emit;
    }
}
