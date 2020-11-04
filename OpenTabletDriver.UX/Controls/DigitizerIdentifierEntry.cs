using System;
using System.Collections.Generic;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;
using OpenTabletDriver.UX.Tools;

namespace OpenTabletDriver.UX.Controls
{
    using static ParseTools;
    
    public class DigitizerIdentifierEntry : DeviceIdentifierEntry<DigitizerIdentifier>
    {
        public DigitizerIdentifierEntry(
            Func<List<DigitizerIdentifier>> getValue,
            Action<List<DigitizerIdentifier>> setValue,
            string name,
            int index,
            Type reportParser
        ) : base(getValue, setValue, name, index, reportParser)
        {
        }

        protected override IEnumerable<Control> GetControls()
        {
            yield return new InputBox("Width (mm)",
                () => GetCurrent().Width.ToString(),
                (o) => ModifyCurrent(id => id.Width = ToFloat(o))
            );
            yield return new InputBox("Height (mm)",
                () => GetCurrent().Height.ToString(),
                (o) => ModifyCurrent(id => id.Height = ToFloat(o))
            );
            yield return new InputBox("Max X (px)",
                () => GetCurrent().MaxX.ToString(),
                (o) => ModifyCurrent(id => id.MaxX = ToFloat(o))
            );
            yield return new InputBox("Max Y (px)",
                () => GetCurrent().MaxY.ToString(),
                (o) => ModifyCurrent(id => id.MaxY = ToFloat(o))
            );
            yield return new InputBox("Max Pressure",
                () => GetCurrent().MaxPressure.ToString(),
                (o) => ModifyCurrent(id => id.MaxPressure = ToUInt(o))
            );
            yield return new InputBox("Active Report ID",
                () => GetCurrent().ActiveReportID?.ToString() ?? new DetectionRange().ToString(),
                (o) => ModifyCurrent(id => id.ActiveReportID = DetectionRange.Parse(o))
            );
            foreach (var control in base.GetControls())
                yield return control;
        }
    }
}