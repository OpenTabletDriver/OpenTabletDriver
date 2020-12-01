using System;
using OpenTabletDriver.Reflection;

namespace OpenTabletDriver.Desktop.Binding
{
    public class BindingReference : PluginReference
    {
        public BindingReference(string bindingPath, string bindingProperty = null)
            : base(AppInfo.PluginManager, bindingPath)
        {
            BindingProperty = bindingProperty;
        }

        public BindingReference(Type type, string bindingProperty = null)
            : this(type.FullName, bindingProperty)
        {
        }

        public string BindingProperty { get; }

        public static BindingReference FromString(string full)
        {
            if (string.IsNullOrWhiteSpace(full))
                return null;
            
            return new BindingReference(
                BindingTools.GetBindingPath(full),
                BindingTools.GetBindingProperty(full)
            );
        }

        public override string ToString()
        {
            return BindingTools.GetBindingString(this.Path, this.BindingProperty);
        }

        public string ToDisplayString()
        {
            return BindingTools.GetBindingString(this.Name, this.BindingProperty);
        }

        public static implicit operator string(BindingReference bindingRef)
        {
            return bindingRef?.ToString();
        }
    }
}
