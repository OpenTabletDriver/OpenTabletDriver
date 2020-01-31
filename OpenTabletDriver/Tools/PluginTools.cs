using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TabletDriverPlugin.Attributes;

namespace OpenTabletDriver.Tools
{
    public static class PluginTools
    {
        public static IEnumerable<(string, string)> GetPluginSettings(object obj)
        {
            if (obj != null)
            {
                foreach (var property in obj.GetType().GetProperties())
                {
                    var attributes = from attr in property.GetCustomAttributes(false)
                        where attr is PropertyAttribute
                        select attr as PropertyAttribute;

                    if (attributes.Count() > 0)
                        yield return (property.Name, property.GetValue(obj).ToString());
                }
            }
        }


        public static bool IsPluginIgnored(this Type type)
        {
            return type.GetCustomAttributes(false).Any(a => a.GetType() == typeof(PluginIgnoreAttribute));
        }
    }
}