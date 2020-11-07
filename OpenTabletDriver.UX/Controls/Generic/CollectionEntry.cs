using System;
using System.Collections;
using Eto.Drawing;
using Eto.Forms;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public abstract class CollectionEntry<T> : StackLayoutItem where T : IEnumerable
    {
        protected CollectionEntry(
            Func<T> getValue,
            Action<T> setValue
        )
        {
            this.getValue = getValue;
            this.setValue = setValue;
            base.Expand = true;
        }

        protected virtual void Build()
        {
            this.deleteButton = new Button((sender, e) => OnDestroy())
            {
                Text = "-"
            };

            base.Control = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = contentAlignment,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem
                    {
                        Control = this.controlContainer,
                        Expand = true
                    },
                    new StackLayoutItem
                    {
                        Control = new Panel
                        {
                            Content = this.deleteButton
                        },
                        VerticalAlignment = deleteButtonAlignment,
                        Expand = false
                    }
                }
            };
        }

        protected virtual void OnDestroy()
        {
            Destroy?.Invoke(this, new EventArgs());
        }

        protected virtual void ModifyValue(Action<T> modify)
        {
            var source = getValue();
            modify(source);
            setValue(source);
        }

        public event EventHandler Destroy;

        protected Func<T> getValue;
        protected Action<T> setValue;
        
        protected Container controlContainer;
        protected Button deleteButton;

        protected VerticalAlignment deleteButtonAlignment = Eto.Forms.VerticalAlignment.Bottom;
        protected VerticalAlignment contentAlignment = Eto.Forms.VerticalAlignment.Center;
    }
}