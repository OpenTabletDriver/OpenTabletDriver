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
        ) : this()
        {
            textBox.TextBinding.Bind(getValue, setValue);
            base.Text = name;
        }

        protected InputBox()
        {
            this.Content = textBox;
        }

        private TextBox textBox = new TextBox();
    }
}