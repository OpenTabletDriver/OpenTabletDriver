using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginManager
    {
        public PluginManager()
        {
            Task.Run(() =>
            {
                pluginTypes = new ConcurrentDictionary<Type, List<Type>>();
                internalImplementations = RetrieveAssemblies();
                LoadImplementableTypes();
                LoadInternalPluginTypes();
                waitHandle.Set();
            });
        }

        private ManualResetEventSlim waitHandle = new ManualResetEventSlim();
        private ConcurrentDictionary<Type, List<Type>> pluginTypes;
        private Assembly[] internalImplementations;
        private Type[] implementableTypes;

        public IEnumerable<Type> PluginTypes => pluginTypes.Values.SelectMany(t => t);

        protected virtual Assembly[] RetrieveAssemblies()
        {
            return new Assembly[] { typeof(Driver).Assembly };
        }

        public IReadOnlyCollection<Type> GetChildTypes<T>()
        {
            var type = typeof(T);
            if (type.IsGenericType)
                type = type.GetGenericTypeDefinition();

            return pluginTypes.TryGetValue(type, out var childTypes) ? childTypes : Array.Empty<Type>();
        }

        public T ConstructObject<T>(string name, params object[] parameters) where T : class
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var objectType = GetChildTypes<T>().FirstOrDefault(t => t.FullName == name);
                try
                {
                    return (T)Activator.CreateInstance(objectType, parameters);
                }
                catch (MissingMethodException e)
                {
                    Log.Write("Plugin", $"No matching constructor found for '{name}'", LogLevel.Error);
                    Log.Write("Plugin", $"{e}", LogLevel.Debug);
                }
                catch (Exception e)
                {
                    Log.Write("Plugin", $"Failed to construct object '{name}'", LogLevel.Error);
                    Log.Write("Plugin", $"{e.InnerException ?? e}", LogLevel.Debug);
                }
            }
            return null;
        }

        public PluginReference GetPluginReference(string path)
        {
            return new PluginReference(this, path);
        }

        public PluginReference GetPluginReference(Type type)
        {
            return new PluginReference(this, type.FullName);
        }

        private void Add(Type pluginType, Type implementedType)
        {
            pluginTypes.AddOrUpdate(implementedType,
                (t) => new List<Type>() { pluginType },
                (t, l) =>
                {
                    l.Add(pluginType);
                    return l;
                }
            );
        }

        public void Add(Type pluginType)
        {
            if (!waitHandle.IsSet)
                waitHandle.Wait();

            foreach (var implementedType in GetImplementedPluginTypes(pluginType))
            {
                Add(pluginType, implementedType);
            }
        }

        public bool Remove(Type pluginType)
        {
            if (!waitHandle.IsSet)
                waitHandle.Wait();

            return GetImplementedPluginTypes(pluginType).All(implementedType =>
            {
                if (pluginTypes.TryGetValue(implementedType, out var list))
                {
                    return list.Remove(pluginType);
                }
                return false;
            });
        }

        private void LoadImplementableTypes()
        {
            implementableTypes = typeof(IDriver).Assembly.ExportedTypes
                .Where(type => type.IsAbstract | type.IsInterface)
                .ToArray();
        }

        private void LoadInternalPluginTypes()
        {
            foreach (var subType in internalImplementations.SelectMany(asm => asm.ExportedTypes))
            {
                foreach (var implementedType in GetImplementedPluginTypes(subType))
                {
                    Add(subType, implementedType);
                }
            }
        }

        private IEnumerable<Type> GetImplementedPluginTypes(Type subType)
        {
            foreach (var implementableType in implementableTypes)
            {
                if (subType.IsAssignableTo(implementableType))
                {
                    yield return implementableType;
                }
                else
                {
                    foreach (var _ in subType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == implementableType))
                    {
                        yield return implementableType;
                    }
                }
            }
        }
    }
}