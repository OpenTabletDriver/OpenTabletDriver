using System;
using System.Collections.Generic;
using System.Text;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class ListEditor : GroupBoxBase
    {
        public ListEditor(
            string name,
            Func<IEnumerable<string>> getValue,
            Action<IEnumerable<string>> setValue
        ) : this()
        {
            textArea.TextBinding.Bind(
                () => 
                {
                    StringBuilder buffer = new StringBuilder();
                    foreach (string value in getValue())
                        buffer.AppendLine(value);
                    return buffer.ToString();
                },
                (o) => 
                {
                    setValue(o.Split(Environment.NewLine));
                }
            );

            base.Text = name;
        }

        protected ListEditor()
        {
            base.Content = textArea;
        }

        private TextArea textArea = new TextArea();
    }
}