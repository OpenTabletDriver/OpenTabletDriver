using System.Linq;
using System.Text.RegularExpressions;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.Desktop.Binding
{
    public static class BindingTools
    {
        public const string BindingRegexExpression = "^(?<BindingPath>.+?): (?<BindingProperty>.+?)$";
    }
}
