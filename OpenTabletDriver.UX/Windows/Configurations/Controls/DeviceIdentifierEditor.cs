using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Windows.Configurations.Controls
{
    public class DeviceIdentifierEditor<T> : CustomItemList<T> where T : DeviceIdentifier
    {
        protected override Control CreateControl(int index, DirectBinding<T> itemBinding)
        {
            var entry = new DeviceIdentifierEntry<T>();
            entry.EntryBinding.Bind(itemBinding);

            return entry;
        }
    }
}