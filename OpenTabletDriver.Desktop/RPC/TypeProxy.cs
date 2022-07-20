using System;
using System.Linq;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Reflection;

#nullable enable

namespace OpenTabletDriver.Desktop.RPC
{
    [JsonObject]
    public class TypeProxy
    {
        [JsonConstructor]
        public TypeProxy()
        {
        }

        public TypeProxy(IPluginFactory pluginFactory, Type type)
        {
            Path = type.Namespace + "." + type.Name;
            FriendlyName = pluginFactory.GetFriendlyName(type.GetPath());
        }

        public string Path { set; get; } = string.Empty;
        public string? FriendlyName { set; get; }

        public Type FindType(IPluginManager pluginManager)
        {
            return pluginManager.ExportedTypes.First(t => t.FullName == Path);
        }
    }
}
