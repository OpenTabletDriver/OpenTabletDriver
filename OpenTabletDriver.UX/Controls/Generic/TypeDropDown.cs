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
            this.ItemTextBinding = Binding.Property<TypeInfo, string>(t => GetFriendlyName(t));
            this.ItemKeyBinding = Binding.Property<TypeInfo, string>(t => t.FullName);

            Refresh();
        }

        public IEnumerable<TypeInfo> Types { protected set; get; }

        public void Refresh()
        {
            this.DataStore = Types = from type in AppInfo.PluginManager.GetChildTypes<T>()
                orderby GetFriendlyName(type)
                select type;
        }

        public event EventHandler<EventArgs> SelectedTypeChanged;

        public TypeInfo SelectedType
        {
            set
            {
                this.SelectedValue = value;
                SelectedTypeChanged?.Invoke(this, new EventArgs());
            }
            get => (TypeInfo)this.SelectedValue;
        }

        public BindableBinding<TypeDropDown<T>, TypeInfo> SelectedTypeBinding
        {
            get
            {
                return new BindableBinding<TypeDropDown<T>, TypeInfo>(
                    this,
                    c => c.SelectedType,
                    (c, v) => c.SelectedType = v,
                    (c, h) => c.SelectedTypeChanged += h,
                    (c, h) => c.SelectedTypeChanged -= h
                );
            }
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