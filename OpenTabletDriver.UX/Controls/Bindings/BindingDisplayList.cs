using System.Collections.Generic;
using System.Collections.Specialized;
using Eto.Forms;
using OpenTabletDriver.Desktop.Reflection;
using OpenTabletDriver.UX.Controls.Generic;

namespace OpenTabletDriver.UX.Controls.Bindings
{
    public class BindingDisplayList : GeneratedItemList<PluginSettingStore>
    {
        public string Prefix { set; get; }

        private IList<string> _buttonNames;
        public IList<string> ButtonNames
        {
            set
            {
                _buttonNames = value;
                if (ItemSource != null)
                    HandleCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            get => _buttonNames;
        }

        protected virtual string GetTextForIndex(int index)
        {
            if (_buttonNames != null && index < _buttonNames.Count
                && !string.IsNullOrEmpty(_buttonNames[index]))
                return _buttonNames[index];
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
