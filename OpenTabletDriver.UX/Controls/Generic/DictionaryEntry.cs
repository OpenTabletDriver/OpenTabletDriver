using System;
using System.Collections.Generic;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class DictionaryEntry : CollectionEntry<Dictionary<string, string>>
    {
        public DictionaryEntry(
            Func<Dictionary<string, string>> getValue,
            Action<Dictionary<string, string>> setValue,
            string startKey = null,
            string startValue = null
        ) : base(getValue, setValue)
        {
            this.startKey = startKey;
            this.startValue = startValue;
            Build();
        }

        protected override void Build()
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
            
            string oldKey = startKey;
            keyBox.TextBinding.Bind(
                () => startKey,
                (key) =>
                {
                    ModifyValue(dict =>
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
                    ModifyValue(dict =>
                    {
                        if (!dict.TryAdd(key, value))
                            dict[key] = value;
                    });
                }
            );

            this.controlContainer = new StackLayout
            {
                Orientation = Orientation.Horizontal,
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
                    }
                }
            };

            base.Build();
        }

        protected override void OnDestroy()
        {
            ModifyValue(dict =>
            {
                if (dict.ContainsKey(this.keyBox.Text))
                    dict.Remove(this.keyBox.Text);
            });
            base.OnDestroy();
        }

        private TextBox keyBox, valueBox;
        private string startKey, startValue;
    }
}