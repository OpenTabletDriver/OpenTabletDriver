using System;
using System.Collections.Generic;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.UX.Windows.Configurations.Controls
{
    public class AuxiliaryIdentifierEditor : DeviceIdentifierEditor<DeviceIdentifier>
    {
        public AuxiliaryIdentifierEditor(
            string name,
            Func<List<DeviceIdentifier>> getValue,
            Action<List<DeviceIdentifier>> setValue
        ) : base(name, getValue, setValue)
        {
        }

        protected override void AddItem(int index)
        {
            var item = new DeviceIdentifierEntry<DeviceIdentifier>(
                base.getValue,
                base.setValue,
                "Auxiliary Identifier",
                index,
                typeof(AuxReportParser)
            );
            base.AddItem(item);
        }
    }
}