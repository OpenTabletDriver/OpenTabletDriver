using System;
using System.Collections.Generic;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class DictionaryEntry : StackLayoutItem
    {
        public DictionaryEntry(
            Dictionary<string, string> source,
            string startKey = null,
            string startValue = null
        ) : this()
        {
            this.dict = source;
            string oldKey = startKey;
            keyBox.TextBinding.Bind(
                () => startKey,
                (key) =>
                {
                    var value = valueBox.Text;
                    if (string.IsNullOrWhiteSpace(key))
                        this.dict.Remove(key);
                    else if (!source.TryAdd(key, value))
                        this.dict[key] = value;
                    
                    if (oldKey != null)
                        this.dict.Remove(oldKey);
                    oldKey = key;
                }
            );

            valueBox.TextBinding.Bind(
                () => startValue,
                (value) =>
                {
                    var key = keyBox.Text;

                    if (!this.dict.TryAdd(key, value))
                        this.dict[key] = value;
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
                        Control = keyBox,
                        Expand = true
                    },
                    new StackLayoutItem
                    {
                        Control = valueBox,
                        Expand = true
                    },
                    new StackLayoutItem
                    {
                        Control = deleteButton,
                        Expand = false
                    }
                }
            };

            base.Expand = true;
        }

        protected void OnDestroy()
        {
            if (this.dict.ContainsKey(this.keyBox.Text))
                this.dict.Remove(this.keyBox.Text);

            this.Destroy?.Invoke(this, new EventArgs());
        }

        private Dictionary<string, string> dict;
        private TextBox keyBox, valueBox;
        private Button deleteButton;

        public event EventHandler Destroy;
    }
}