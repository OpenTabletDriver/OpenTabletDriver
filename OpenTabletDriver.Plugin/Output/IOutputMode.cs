using System.Collections.Generic;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin.Output
{
    public interface IOutputMode
    {
        /// <summary>
        /// Sends a report to be processed and handled by the <see cref="IOutputMode"/>.
        /// </summary>
        /// <param name="report"></param>
        void Read(IDeviceReport report);

        /// <summary>
        /// The list of <see cref="IFilter"/>s in which the final output is filtered.
        /// </summary>
        IList<IFilter> Filters { set; get; }
        
        /// <summary>
        /// The current tablet assigned to this <see cref="IOutputMode"/>
        /// </summary>
        TabletState Tablet { set; get; }
    }
}
