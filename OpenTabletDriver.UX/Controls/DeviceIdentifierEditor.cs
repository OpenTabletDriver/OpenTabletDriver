using System;
using System.Collections.Generic;
using Eto.Forms;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls
{
    public abstract class DeviceIdentifierEditor<T> : CollectionEditor<List<T>> where T : DeviceIdentifier, new()
    {
        public DeviceIdentifierEditor(
            string name,
            Func<List<T>> getValue,
            Action<List<T>> setValue
        ) : base(name, getValue, setValue)
        {
            base.Content = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Spacing = 5,
                Items =
                {
                    new StackLayoutItem(entries, HorizontalAlignment.Stretch),
                    new StackLayoutItem(actions, HorizontalAlignment.Right)
                }
            };
        }

        protected abstract void AddItem(int index);

        protected override void CreateItem()
        {
            var list = base.getValue();
            list.Add(new T());
            base.setValue(list);

            AddItem(list.Count - 1);
        }

        protected override void Refresh()
        {
            entries.Items.Clear();
            for (int i = 0; i < base.getValue().Count; i++)
                AddItem(i);
        }

        protected override void RemoveItem(object sender, EventArgs e)
        {
            Refresh();
        }
    }
}