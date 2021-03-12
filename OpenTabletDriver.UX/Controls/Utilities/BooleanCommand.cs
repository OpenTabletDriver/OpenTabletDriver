using System;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Utilities
{
    public class BooleanCommand : CheckCommand
    {
        public BooleanCommand()
        {
            CheckedBinding = new BindableBinding<BooleanCommand, bool?>(
                this,
                (c) => c.Checked,
                (c, v) => c.Checked = v ?? false,
                (c, h) => c.CheckedChanged += h,
                (c, h) => c.CheckedChanged -= h
            );
        }

        public BindableBinding<BooleanCommand, bool?> CheckedBinding { get; }
    }
}
