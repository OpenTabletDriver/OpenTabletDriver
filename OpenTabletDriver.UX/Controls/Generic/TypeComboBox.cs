using System;
using System.Collections.Generic;
using System.Reflection;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.Plugin.Attributes;

namespace OpenTabletDriver.UX.Controls.Generic
{
    public class TypeComboBox<T> : ComboBox where T : class
    {
        public TypeComboBox()
        {
            this.DataStore = Types = AppInfo.PluginManager.GetChildTypes<T>();
            this.ItemTextBinding = Binding.Property<TypeInfo, string>(t => GetFriendlyName(t));
            this.ItemKeyBinding = Binding.Property<TypeInfo, string>(t => t.FullName);
        }

        public IReadOnlyCollection<TypeInfo> Types { get; }

        public T ConstructSelectedType(params object[] args)
        {
            args ??= Array.Empty<object>();
            var type = this.SelectedValue as TypeInfo;
            return AppInfo.PluginManager.ConstructObject<T>(type.FullName, args);
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