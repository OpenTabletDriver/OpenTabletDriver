using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenTabletDriver
{
    public class InputDeviceTreeList : Collection<InputDeviceTree>
    {
        public new void Clear()
        {
            var outdatedDevices = new List<InputDevice>();
            foreach (var tree in base.Items)
                foreach (var dev in tree.InputDevices)
                    outdatedDevices.Add(dev);
            
            foreach (var dev in outdatedDevices)
                dev.Dispose();
            
            base.Clear();
        }
    }
}
