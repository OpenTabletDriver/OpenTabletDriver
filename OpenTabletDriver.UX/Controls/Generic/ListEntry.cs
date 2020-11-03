using System;
using System.Collections.Generic;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class ListEntry : StackLayoutItem
    {
        public ListEntry(
            Func<IList<string>> getValue,
            Action<IList<string>> setValue,
            int index
        ) : this()
        {
            this.Index = index;
            this.getValue = getValue;
            this.setValue = setValue;

            this.textBox.TextBinding.Bind(
                () =>
                {
                    var source = getValue();
                    if (source.Count > index)
                        return source[this.Index];
                    else
                        return string.Empty;
                },
                (s) =>
                {
                    ModifyValue(source =>
                    {
                        if (source.Count > index)
                            source[this.Index] = s;
                        else
                            source.Add(s);
                    });
                }
            );
        }

        protected ListEntry()
        {
            this.textBox = new TextBox();

            this.deleteButton = new Button((sender, e) => OnDestroy())
            {
                Text = "-"
            };

            base.Control = new StackLayout
            {
                Spacing = 5,
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = Eto.Forms.VerticalAlignment.Center,
                Items =
                {
                    new StackLayoutItem
                    {
                        Control = this.textBox,
                        Expand = true
                    },
                    new StackLayoutItem
                    {
                        Control = this.deleteButton,
                        Expand = false
                    }
                }
            };
        }

        public event EventHandler Destroy;

        public int Index { private set; get; }

        private void OnDestroy()
        {
            ModifyValue(source =>
            {
                if (source.Count > this.Index)
                    source.RemoveAt(this.Index);
            });
            Destroy?.Invoke(this, new EventArgs());
        }

        private void ModifyValue(Action<IList<string>> modifyMethod)
        {
            var source = getValue();
            modifyMethod(source);
            setValue(source);
        }

        private Func<IList<string>> getValue;
        private Action<IList<string>> setValue;

        private TextBox textBox;
        private Button deleteButton;
    }
}