using System;
using System.Collections.Generic;
using OpenTabletDriver.Plugin.Tablet;

namespace OpenTabletDriver.UX.Windows.Configurations.Controls
{
    public class DigitizerIdentifierEditor : DeviceIdentifierEditor<DigitizerIdentifier>
    {
        public DigitizerIdentifierEditor(
            string name,
            Func<List<DigitizerIdentifier>> getValue,
            Action<List<DigitizerIdentifier>> setValue
        ) : base(name, getValue, setValue)
        {
        }

        protected override void AddItem(int index)
        {
            var item = new DigitizerIdentifierEntry(
                base.getValue,
                base.setValue,
                "Digitizer Identifier",
                index,
                typeof(DigitizerIdentifier)
            );
            base.AddItem(item);
        }
    }
}