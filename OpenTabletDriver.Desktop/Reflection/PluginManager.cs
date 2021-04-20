using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.DependencyInjection;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginManager : ServiceManager
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

        public IEnumerable<Type> PluginTypes
        {
            get
            {
                WaitUntilReady();
                return pluginTypes.Values.SelectMany(t => t);
            }
        }

        protected virtual Assembly[] RetrieveAssemblies()
        {
            return new Assembly[] { typeof(Driver).Assembly };
        }

        public IReadOnlyCollection<Type> GetChildTypes<T>()
        {
            WaitUntilReady();
            var type = typeof(T);
            if (type.IsGenericType)
                type = type.GetGenericTypeDefinition();

            return pluginTypes.TryGetValue(type, out var childTypes) ? childTypes : Array.Empty<Type>();
        }

        public T ConstructObject<T>(string name, params object[] parameters) where T : class
        {
            WaitUntilReady();
            if (!string.IsNullOrWhiteSpace(name))
            {
                var objectType = GetChildTypes<T>().FirstOrDefault(t => t.FullName == name);
                try
                {
                    var obj = (T)Activator.CreateInstance(objectType, parameters);

                    if (obj != null)
                    {
                        var resolvedProperties = from property in objectType.GetProperties()
                            where property.GetCustomAttribute<ResolvedAttribute>() is ResolvedAttribute
                            select property;

                        foreach (var property in resolvedProperties)
                        {
                            var service = GetService(property.PropertyType);
                            if (service != null)
                                property.SetValue(obj, service);
                        }

                        var resolvedFields = from field in objectType.GetFields()
                            where field.GetCustomAttribute<ResolvedAttribute>() is ResolvedAttribute
                            select field;

                        foreach (var field in resolvedFields)
                        {
                            var service = GetService(field.FieldType);
                            if (service != null)
                                field.SetValue(obj, service);
                        }
                    }

                    return obj;
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
            WaitUntilReady();
            return new PluginReference(this, path);
        }

        public PluginReference GetPluginReference(Type type)
        {
            return GetPluginReference(type.FullName);
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
            WaitUntilReady();
            foreach (var implementedType in GetImplementedPluginTypes(pluginType))
            {
                Add(pluginType, implementedType);
            }
        }

        public bool Remove(Type pluginType)
        {
            WaitUntilReady();
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

        private void WaitUntilReady()
        {
            if (!waitHandle.IsSet)
                waitHandle.Wait();
        }
    }
}