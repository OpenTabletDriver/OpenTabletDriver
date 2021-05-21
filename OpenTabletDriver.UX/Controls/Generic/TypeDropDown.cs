using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Plugin.Attributes;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class TypeDropDown<T> : DropDown<TypeInfo> where T : class
    {
        public TypeDropDown()
        {
            this.ItemTextBinding = Binding.Property<TypeInfo, string>(t => GetFriendlyName(t));
            this.ItemKeyBinding = Binding.Property<TypeInfo, string>(t => t.FullName);

            Refresh();
        }

        public IList<TypeInfo> Types { protected set; get; }

        public void Refresh()
        {
            var newTypes = from type in AppInfo.PluginManager.GetChildTypes<T>()
                orderby GetFriendlyName(type)
                select type;
            this.DataStore = Types = newTypes.ToList();
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            if (SelectedIndex >= 0)
                SelectedItem = Types[SelectedIndex];
        }

        public T ConstructSelectedType(params object[] args)
        {
            if (SelectedItem != null)
            {
                args ??= Array.Empty<object>();
                var pluginRef = AppInfo.PluginManager.GetPluginReference(SelectedItem);
                return pluginRef.Construct<T>();
            }
            return null;
        }

        public void Select(Func<T, bool> predicate)
        {
            foreach (TypeInfo type in Types)
            {
                var obj = AppInfo.PluginManager.ConstructObject<T>(type.FullName);
                if (predicate(obj))
                {
                    this.SelectedValue = type;
                    break;
                }
            }
        }

        protected string GetFriendlyName(TypeInfo t)
        {
            return t.GetCustomAttribute<PluginNameAttribute>()?.Name ?? t.FullName;
        }
    }
}