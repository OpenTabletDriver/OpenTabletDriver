using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace OpenTabletDriver
{
    /// <summary>
    /// A collection of <see cref="InputDevice"/>s.
    /// </summary>
    [PublicAPI]
    public class InputDeviceCollection : Collection<InputDevice>
    {
        protected override void ClearItems()
        {
            var outdatedDevices = new List<InputDeviceEndpoint>();
            foreach (var tree in base.Items)
                foreach (var dev in tree.Endpoints)
                    outdatedDevices.Add(dev);

            foreach (var dev in outdatedDevices)
                dev.Dispose();

            base.ClearItems();
        }
    }
}
