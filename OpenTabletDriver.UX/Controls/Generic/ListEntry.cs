using System;
using System.Collections.Generic;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class ListEntry : CollectionEntry<IList<string>>
    {
        public ListEntry(
            Func<IList<string>> getValue,
            Action<IList<string>> setValue,
            int index
        ) : base(getValue, setValue)
        {
            this.Index = index;
            Build();
        }

        public int Index { private set; get; }

        protected override void Build()
        {
            this.textBox = new TextBox();
            this.textBox.TextBinding.Bind(
                () =>
                {
                    var source = getValue();
                    if (source.Count > this.Index)
                        return source[this.Index];
                    else
                        return string.Empty;
                },
                (s) =>
                {
                    ModifyValue(source =>
                    {
                        if (source.Count > this.Index)
                            source[this.Index] = s;
                        else
                            source.Add(s);
                    });
                }
            );

            base.controlContainer = new Panel
            {
                Content = this.textBox
            };

            base.Build();
        }

        protected override void OnDestroy()
        {
            ModifyValue(source =>
            {
                if (source.Count > this.Index)
                    source.RemoveAt(this.Index);
            });
            base.OnDestroy();
        }

        private TextBox textBox;
    }
}