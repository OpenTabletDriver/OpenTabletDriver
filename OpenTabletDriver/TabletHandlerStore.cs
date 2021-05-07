using System;
using System.Collections.Generic;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver
{
    public class TabletHandlerStore : IDisposable
    {
        private Dictionary<TabletHandlerID, TabletHandler> TabletHandlers = new Dictionary<TabletHandlerID, TabletHandler>();
        public TabletHandler this[TabletHandlerID ID]
        {
            get => TabletHandlers.TryGetValue(ID, out var tabletHandler) ? tabletHandler : null;
        }

        public void Add(TabletHandler handler)
        {
            if (!TabletHandlers.TryAdd(handler.InstanceID, handler))
            {
                // Do not allow implicit overwriting of TabletHandler instance references
                throw new Exception($"{handler.InstanceID} already exists");
            }
        }

        public bool Remove(TabletHandlerID ID)
        {
            TabletHandlers[ID].Dispose();
            return TabletHandlers.Remove(ID);
        }

        public IEnumerable<TabletHandler> GetTabletHandlers()
        {
            return TabletHandlers.Values;
        }

        public void Dispose()
        {
            foreach (var tabletHandler in TabletHandlers.Values)
                tabletHandler.Dispose();
        }
    }
}