using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Plugin.Attributes;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class TypeDropDown<T> : DropDown where T : class
    {
        public TypeDropDown()
        {
            this.ItemTextBinding = Binding.Property<Type, string>(t => GetFriendlyName(t));
            this.ItemKeyBinding = Binding.Property<Type, string>(t => t.FullName);

            Refresh();
        }

        public IEnumerable<Type> Types { protected set; get; }

        public void Refresh()
        {
            this.DataStore = Types = from type in AppInfo.PluginManager.GetChildTypes<T>()
                orderby GetFriendlyName(type)
                select type;
        }

        public Type SelectedType
        {
            set => this.SelectedValue = value;
            get => (Type)this.SelectedValue;
        }

        public T ConstructSelectedType(params object[] args)
        {
            if (SelectedType != null)
            {
                args ??= Array.Empty<object>();
                var pluginRef = AppInfo.PluginManager.GetPluginReference(SelectedType);
                return pluginRef.Construct<T>();
            }
            return null;
        }

        public void Select(Func<T, bool> predicate)
        {
            foreach (Type type in Types)
            {
                var obj = AppInfo.PluginManager.ConstructObject<T>(type.FullName);
                if (predicate(obj))
                {
                    this.SelectedValue = type;
                    break;
                }
            }
        }

        protected string GetFriendlyName(Type t)
        {
            return t.GetCustomAttribute<PluginNameAttribute>()?.Name ?? t.FullName;
        }
    }
}