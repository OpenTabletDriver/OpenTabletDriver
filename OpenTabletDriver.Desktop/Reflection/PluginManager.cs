using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;
using OpenTabletDriver.Plugin;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginManager
    {
        public PluginManager()
        {
            pluginLoadTask = taskFactory.RunAsync(async () =>
            {
                await Task.Run(() =>
                {
                    internalImplementations = RetrieveAssemblies();
                    LoadImplementableTypes();
                    LoadInternalPluginTypes();
                });
            });
        }

        private readonly JoinableTask pluginLoadTask;
        private readonly JoinableTaskFactory taskFactory = new JoinableTaskFactory(new JoinableTaskContext());
        private readonly Dictionary<Type, List<Type>> pluginTypes = new Dictionary<Type, List<Type>>();
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

        private static void Add(Type implementedType, Type subType, Dictionary<Type, List<Type>> store)
        {
            if (!store.TryGetValue(implementedType, out var list))
            {
                list = new List<Type>();
                store.Add(implementedType, list);
            }

            if (!list.Contains(subType))
            {
                list.Add(subType);
            }
        }

        public async Task Add(Type pluginType)
        {
            await pluginLoadTask;

            foreach (var implementedType in GetImplementedPluginTypes(pluginType))
            {
                Add(implementedType, pluginType, pluginTypes);
            }
        }

        public async Task<bool> Remove(Type pluginType)
        {
            await pluginLoadTask;

            bool ret = false;
            foreach (var implementedType in GetImplementedPluginTypes(pluginType))
            {
                if (pluginTypes.TryGetValue(implementedType, out var list))
                {
                    ret = list.Remove(pluginType);
                }
            }
            return ret;
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
                    Add(implementedType, subType, pluginTypes);
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

        public class CacheTypeException : Exception
        {
            public CacheTypeException(string msg) : base(msg)
            {
            }
        }
    }
}