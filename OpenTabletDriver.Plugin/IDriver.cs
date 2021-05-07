using System;
using System.Collections.Generic;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.Plugin
{
    public interface IDriver : IDisposable
    {
        /// <summary>
        /// Populates OTD's internal tablet handler list using the specified <see cref="IEnumerable{TabletConfiguration}"/>
        /// </summary>
        /// <param name="tabletConfigurations"></param>
        void EnumerateTablets(IEnumerable<TabletConfiguration> tabletConfigurations);

        /// <summary>
        /// Process specific devices and create tablet handlers if possible
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="device"></param>
        void ProcessDevices(IEnumerable<object> device, IEnumerable<TabletConfiguration> tabletConfigurations);

        /// <summary>
        /// Retrieves the IDs of the currently active tablet handllers
        /// </summary>
        /// <returns>An enumerable </returns>
        IEnumerable<TabletHandlerID> GetActiveTabletHandlerIDs();

        /// <summary>
        /// Sets the output mode of a TabletHandler associated with <see cref="TabletHandlerID"/>
        /// </summary>
        /// <param name="ID"></param>
        void SetOutputMode(TabletHandlerID ID, IOutputMode outputMode);

        /// <summary>
        /// Gets the output mode of a TabletHandler associated with <see cref="TabletHandlerID"/>
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        IOutputMode GetOutputMode(TabletHandlerID ID);

        /// <summary>
        /// Gets the <see cref="TabletState"/> of a TabletHandler associated with <see cref="TabletHandlerID"/>
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        TabletState GetTabletState(TabletHandlerID ID);

        /// <summary>
        /// Is raised whenever a new TabletHandler is created with an assigned <see cref="TabletHandlerID"/>
        /// </summary>
        event EventHandler<TabletHandlerID> TabletHandlerCreated;

        /// <summary>
        /// Is raised whenever an existing TabletHandler is destroyed either due to an unhandled exception or disconnection
        /// </summary>
        event EventHandler<TabletHandlerID> TabletHandlerDestroyed;
    }
}
