using System;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class InputBox : GroupBoxBase
    {
        public InputBox(
            string name,
            Func<string> getValue,
            Action<string> setValue,
            string placeholder = null
        )
        {
            var textBox = new TextBox
            {
                PlaceholderText = placeholder
            };
            textBox.TextBinding.Bind(getValue, setValue);
            base.Content = textBox;

            base.Text = name;
        }
    }
}