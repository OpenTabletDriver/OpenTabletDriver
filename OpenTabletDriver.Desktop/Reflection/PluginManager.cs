using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginManager : ServiceManager
    {
        public PluginManager()
        {
            var assemblies = new[]
            {
                Assembly.Load("OpenTabletDriver.Desktop"),
                Assembly.Load("OpenTabletDriver.Configurations"),
                Assembly.Load("OpenTabletDriver.Plugin")
            };

            _internalTypes = [.. from asm in assemblies
                                from type in asm.DefinedTypes
                                where type.IsPublic && !(type.IsInterface || type.IsAbstract)
                                where IsPluginType(type)
                                where IsPlatformSupported(type)
                                select type];

            ResetPluginTypes();
        }

        public IReadOnlyCollection<TypeInfo> PluginTypes => pluginTypes;
        protected ConcurrentBag<TypeInfo> pluginTypes;
        private readonly IReadOnlyCollection<TypeInfo> _internalTypes;

        [MemberNotNull(nameof(pluginTypes))]
        private void ResetPluginTypes() => pluginTypes = [.. _internalTypes];

        public virtual T? ConstructObject<T>(string name, object[]? args = null) where T : class
        {
            args ??= [];
            if (!string.IsNullOrWhiteSpace(name))
            {
                try
                {
                    if (PluginTypes.FirstOrDefault(t => t.FullName == name) is TypeInfo type)
                    {
                        var matchingConstructors = from ctor in type.GetConstructors()
                                                   let parameters = ctor.GetParameters()
                                                   where parameters.Length == args.Length
                                                   where IsValidParameterFor(args, parameters)
                                                   select ctor;

                        if (matchingConstructors.FirstOrDefault() is ConstructorInfo constructor)
                        {
                            T obj = (T)constructor.Invoke(args);

                            if (obj != null)
                                Inject(this, obj, type);
                            return obj;
                        }
                        else
                        {
                            Log.Write("Plugin", $"No constructor found for '{name}'", LogLevel.Error);
                        }
                    }
                    else
                    {
                        Log.Write("Plugin", $"Unable to find plugin matching name '{name}'", LogLevel.Error);
                    }
                }
                catch (TargetInvocationException e) when (e.Message == "Exception has been thrown by the target of an invocation.")
                {
                    Log.Write("Plugin", "Object construction has thrown an error", LogLevel.Error);
                    Log.Exception(e.InnerException);
                }
                catch (Exception e)
                {
                    Log.Write("Plugin", $"Unable to construct object '{name}'", LogLevel.Error);
                    Log.Exception(e);
                }
            }
            return null;
        }

        /// <summary>
        /// Get a <see cref="TypeInfo"/> collection of registered plugins implementing type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type implemented by the plugin</typeparam>
        /// <returns>A collection of <see cref="TypeInfo"/></returns>
        public virtual IReadOnlyCollection<TypeInfo> GetChildTypes<T>()
        {
            var children = from type in PluginTypes
                           where typeof(T).IsAssignableFrom(type)
                           where !IsPluginIgnored(type)
                           select type;

            return children.ToArray();
        }

        /// <summary>
        /// Gets the associated <see cref="PluginNameAttribute.Name"/> for a plugin.
        /// </summary>
        /// <param name="path">The path of the plugin (typically FullName of the type)</param>
        /// <returns>The plugins name if the plugin path was found in the registered plugin types</returns>
        /// <remarks>This is somewhat slow, as it first looks up the plugin type in the local cache, and then it reflects the <see cref="PluginNameAttribute"/> out of the type.</remarks>
        public virtual string? GetFriendlyName(string path)
        {
            if (PluginTypes.FirstOrDefault(t => t.FullName == path) is TypeInfo plugin)
            {
                var attrs = plugin.GetCustomAttributes(true);
                var nameattr = attrs.FirstOrDefault(t => t.GetType() == typeof(PluginNameAttribute));
                if (nameattr is PluginNameAttribute attr)
                    return attr.Name;
            }
            return null;
        }

        public static void Inject(IServiceProvider serviceProvider, object? obj)
        {
            if (obj != null)
                Inject(serviceProvider, obj, obj.GetType());
        }

        public static void Inject(IServiceProvider serviceProvider, object? obj, Type type)
        {
            if (obj == null)
                return;

            var resolvedProperties = from property in type.GetProperties()
                                     where property.GetCustomAttribute<ResolvedAttribute>() is ResolvedAttribute
                                     select property;

            foreach (var property in resolvedProperties)
            {
                var service = serviceProvider.GetService(property.PropertyType);
                if (service != null)
                    property.SetValue(obj, service);
            }

            var resolvedFields = from field in type.GetFields()
                                 where field.GetCustomAttribute<ResolvedAttribute>() is ResolvedAttribute
                                 select field;

            foreach (var field in resolvedFields)
            {
                var service = serviceProvider.GetService(field.FieldType);
                if (service != null)
                    field.SetValue(obj, service);
            }
        }

        public override void ResetServices()
        {
            ResetPluginTypes();
            base.ResetServices();
        }

        protected virtual bool IsValidParameterFor(object[] args, ParameterInfo[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var arg = args[i];
                if (!parameter.ParameterType.IsAssignableFrom(arg.GetType()))
                    return false;
            }
            return true;
        }


        /// <summary>
        /// Redirect for <see cref="M:OpenTabletDriver.Desktop.Extensions.IsPluginType(System.Type)"/>
        /// </summary>
        protected virtual bool IsPluginType(Type type) => type.IsPluginType();

        /// <summary>
        /// Check if plugin type has <see cref="SupportedPlatformAttribute"/> and filter based on the current platform.
        /// </summary>
        /// <param name="type">Type of the plugin.</param>
        /// <returns><c>true</c> if current platform is supported, or if the plugin does not define <see cref="SupportedPlatformAttribute"/></returns>
        protected virtual bool IsPlatformSupported(Type type)
            => type.GetCustomAttribute<SupportedPlatformAttribute>(false)?.IsCurrentPlatform ?? true;

        /// <summary>
        /// Check for whether the provided type implements <see cref="PluginIgnoreAttribute"/>.
        /// </summary>
        /// <param name="type">Type of the plugin.</param>
        /// <returns><c>true</c> if the plugin is ignored.</returns>
        protected virtual bool IsPluginIgnored(Type type)
        {
            return type.GetCustomAttributes(false).Any(a => a.GetType() == typeof(PluginIgnoreAttribute));
        }

        protected virtual bool IsLoadable(Assembly asm) => asm.IsLoadable();
    }
}
