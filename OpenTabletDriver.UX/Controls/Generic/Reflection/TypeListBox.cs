using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.UX.Controls.Generic.Reflection
{
    public class TypeListBox<T> : ListBox<TypeInfo> where T : class
    {
        public TypeListBox()
        {
            this.ItemTextBinding = Binding.Property<TypeInfo, string>(t => t.GetFriendlyName());
            this.ItemKeyBinding = Binding.Property<TypeInfo, string>(t => t.FullName);

            AppInfo.PluginManager.AssembliesChanged += HandleAssembliesChanged;
            // Manual update of the DataStore seems to be required, however it isn't on DropDown. Bug?
            this.DataStore = CreateDefaultDataStore();
        }

        public T ConstructSelectedType(params object[] args)
        {
            if (SelectedItem != null)
            {
                args ??= Array.Empty<object>();
                return AppInfo.PluginManager.ConstructObject<T>(SelectedItem.FullName);
            }
            return null;
        }

        public void Select(Func<T, bool> predicate)
        {
            foreach (TypeInfo type in DataStore)
            {
                var obj = AppInfo.PluginManager.ConstructObject<T>(type.FullName);
                if (predicate(obj))
                {
                    this.SelectedValue = type;
                    break;
                }
            }
        }

        protected override IEnumerable<object> CreateDefaultDataStore()
        {
            var query = from type in AppInfo.PluginManager.GetChildTypes<T>()
                orderby type.GetFriendlyName()
                select type;
            return query.ToList();
        }

        private void HandleAssembliesChanged(object sender, EventArgs e) => Application.Instance.AsyncInvoke(() => this.DataStore = CreateDefaultDataStore());
    }
}