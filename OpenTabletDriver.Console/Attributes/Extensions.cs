using System;
using System.Linq;
using System.Reflection;

namespace OpenTabletDriver.Console.Attributes
{
    public static class Extensions
    {
        public static T? GetCustomAttribute<T>(this ICustomAttributeProvider provider) where T : Attribute
        {
            var attrs = provider.GetCustomAttributes(true);
            var attr = attrs.FirstOrDefault(a => a is T);
            return (T?)attr;
        }
    }
}
