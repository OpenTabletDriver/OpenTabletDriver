using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Bindings
{
    public class BindingDisplayList : GeneratedItemList<PluginSettingStore>
    {
        public string Prefix { set; get; }

        protected virtual string GetTextForIndex(int index)
        {
            return $"{Prefix} {index + 1}";
        }

        protected override Control CreateControl(int index, DirectBinding<PluginSettingStore> itemBinding)
        {
            BindingDisplay display = new BindingDisplay();
            display.StoreBinding.Bind(itemBinding);

            return new Group
            {
                Text = GetTextForIndex(index),
                Orientation = Orientation.Horizontal,
                ExpandContent = false,
                Content = display
            };
        }
    }
}
