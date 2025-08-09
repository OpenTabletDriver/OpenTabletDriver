using System;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class InputBox : Group
    {
        public InputBox(
            string name,
            Func<string> getValue,
            Action<string> setValue,
            string placeholder = null,
            int textboxWidth = 300
        )
        {
            this.Orientation = Orientation.Horizontal;
            this.ExpandContent = false;

            var textBox = new TextBox
            {
                Width = textboxWidth,
                PlaceholderText = placeholder
            };
            textBox.TextBinding.Bind(getValue, setValue);

            base.Content = textBox;
            base.Text = name;
        }
    }
}
