using System;
using OpenTabletDriver.Plugin.Output.Async;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Output
{
    public interface IOutputMode : IDisposable
    {
        /// <summary>
        /// Sends a report to be processed and handled by the <see cref="IOutputMode"/>.
        /// </summary>
        /// <param name="report"></param>
        void Read(IDeviceReport report);

        /// <summary>
        /// The configuration for the output mode.
        /// </summary>
        OutputModeConfig Config { get; set; }

        /// <summary>
        /// The current tablet assigned to this <see cref="IOutputMode"/>
        /// </summary>
        TabletState Tablet { get; set; }

        /// <summary>
        /// The class that handles the operation of a platform-specific pointer.
        /// </summary>
        IPointer Pointer { get; }

        /// <summary>
        /// The list of <see cref="IFilter"/>s in which the final output is filtered.
        /// </summary>
        IFilter[] Filters { get; set; }

        /// <summary>
        /// The class that processes input asynchronous to tablet input stream.
        /// </summary>
        AsyncFilterHandler AsyncHandler { get; set; }
    }
}
