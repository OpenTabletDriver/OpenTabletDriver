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
                        yield return (obj.GetType().FullName + "." + property.Name, property.GetValue(obj).ToString());
                }
            }
        }

        public static void SetPluginSettings<T>(T obj, IDictionary<string, string> pluginSettings)
        {
            if (obj != null)
            {
                foreach (var property in obj.GetType().GetProperties())
                {
                    var attributes = from attr in property.GetCustomAttributes(false)
                        where attr is PropertyAttribute
                        select attr as PropertyAttribute;

                    if (pluginSettings.TryGetValue(obj.GetType().FullName + "." + property.Name, out var stringValue))
                    {
                        var value = Convert.ChangeType(stringValue, property.PropertyType);
                        property.SetValue(obj, value);
                    }
                }
            }
        }

        public static bool IsPluginIgnored(this Type type)
        {
            return type.GetCustomAttributes(false).Any(a => a.GetType() == typeof(PluginIgnoreAttribute));
        }
    }
}