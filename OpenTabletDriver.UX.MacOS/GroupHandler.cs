using System;
using Eto.Drawing;

namespace OpenTabletDriver.UX.MacOS;

internal class GroupBoxHandler : Eto.Mac.Forms.Controls.GroupBoxHandler
{
    protected override SizeF GetNaturalSize(SizeF availableSize)
    {
        var baseSize = base.GetNaturalSize(availableSize);
        return new SizeF(Math.Max(baseSize.Width, Font.MeasureString(Text).Width), baseSize.Height);
    }
}
