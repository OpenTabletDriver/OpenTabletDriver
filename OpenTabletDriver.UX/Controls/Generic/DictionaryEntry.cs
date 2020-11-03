using System;
using System.Collections.Generic;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class DictionaryEntry : StackLayoutItem
    {
        public DictionaryEntry(
            Func<Dictionary<string, string>> getValue,
            Action<Dictionary<string, string>> setValue,
            string startKey = null,
            string startValue = null
        ) : this()
        {
            this.getValue = getValue;
            this.setValue = setValue;

            string oldKey = startKey;
            keyBox.TextBinding.Bind(
                () => startKey,
                (key) =>
                {
                    Modify(dict =>
                    {
                        var value = valueBox.Text;
                        if (string.IsNullOrWhiteSpace(key))
                            dict.Remove(key);
                        else if (!dict.TryAdd(key, value))
                            dict[key] = value;

                        if (oldKey != null)
                            dict.Remove(oldKey);
                        oldKey = key;
                    });
                }
            );

            valueBox.TextBinding.Bind(
                () => startValue,
                (value) =>
                {
                    var key = keyBox.Text;
                    Modify(dict =>
                    {
                        if (!dict.TryAdd(key, value))
                            dict[key] = value;
                    });
                }
            );
        }

        protected DictionaryEntry()
        {
            this.keyBox = new TextBox
            {
                PlaceholderText = "Key",
                ToolTip =
                    "The dictionary entry's key. This is what is indexed to find a value." + Environment.NewLine +
                    "If left empty, the entry will be removed on save or apply."
            };

            this.valueBox = new TextBox
            {
                PlaceholderText = "Value",
                ToolTip = "The dictionary entry's value. This is what is retrieved when indexing with the specified key."
            };

            this.deleteButton = new Button((sender, e) => OnDestroy())
            {
                Text = "-"
            };

            base.Control = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = Eto.Forms.VerticalAlignment.Center,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem
                    {
                        Control = this.keyBox,
                        Expand = true
                    },
                    new StackLayoutItem
                    {
                        Control = this.valueBox,
                        Expand = true
                    },
                    new StackLayoutItem
                    {
                        Control = this.deleteButton,
                        Expand = false
                    }
                }
            };

            base.Expand = true;
        }

        protected void OnDestroy()
        {
            Modify(dict =>
            {
                if (dict.ContainsKey(this.keyBox.Text))
                    dict.Remove(this.keyBox.Text);
            });

            this.Destroy?.Invoke(this, new EventArgs());
        }

        private Func<Dictionary<string, string>> getValue;
        private Action<Dictionary<string, string>> setValue;

        protected void Modify(Action<Dictionary<string, string>> modify)
        {
            var dict = getValue();
            modify(dict);
            setValue(dict);
        }

        private TextBox keyBox, valueBox;
        private Button deleteButton;

        public event EventHandler Destroy;
    }
}