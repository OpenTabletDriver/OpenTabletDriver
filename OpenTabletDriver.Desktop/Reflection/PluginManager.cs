using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Autofac;
using OpenTabletDriver.Desktop.Interop.Timer;
using OpenTabletDriver.Desktop.Reflection.Autofac;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Platform.Display;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Platform.Pointer;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Timers;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginManager
    {
        internal ContainerBuilder ContainerBuilder;
        internal Assembly[] assemblies =
        [
            Assembly.Load("OpenTabletDriver.Desktop"),
            Assembly.Load("OpenTabletDriver.Configurations"),
            Assembly.Load("OpenTabletDriver.Plugin"),
        ];

        public PluginManager()
        {
            TypeInfo[] localInternalTypes =
            [
                ..
                from asm in assemblies
                from type in asm.DefinedTypes
                where type.IsPublic && !(type.IsInterface || type.IsAbstract)
                where IsPluginType(type)
                where IsPlatformSupported(type)
                select type
            ];

            if (localInternalTypes.Count(x => x.ImplementedInterfaces.Contains(typeof(ITimer))) > 1)
            {
                // remove FallbackTimer if another timer is present
                localInternalTypes =
                    [.. localInternalTypes.Where(x => x.FullName != typeof(FallbackTimer).GetTypeInfo().FullName)];
            }
            else
                localInternalTypes = [.. localInternalTypes];

            pluginTypes = new ConcurrentBag<TypeInfo>(localInternalTypes);

            RegisterContainer();
            Debug.Assert(ContainerBuilder != null, "ContainerBuilder should be initialized at the end of constructor");
        }

        public IReadOnlyCollection<TypeInfo> PluginTypes => pluginTypes;
        protected ConcurrentBag<TypeInfo> pluginTypes;

        private Type[] _bannedAutoloadTypes =
        [
            typeof(IDeviceReport), // skip parser structs as they don't make sense to DI
            typeof(IVirtualScreen), // handled with module
        ];

        // types that should have 1 instance per scope
        private Type[] _scopedAutoloadTypes =
        [
            typeof(IAbsolutePointer),
            typeof(IRelativePointer),
            typeof(IVirtualPad),
            typeof(IVirtualKeyboard),
            typeof(IPressureHandler),
        ];

        protected virtual void RegisterContainer()
        {
            ContainerBuilder = new();
            foreach (var t in pluginTypes)
            {
                if (_bannedAutoloadTypes.Any(autoloadType => t.IsAssignableTo(autoloadType)))
                    continue; // skip banned types

                Debug.Assert(!string.IsNullOrEmpty(t.FullName));

                Type keyType = t.ImplementedInterfaces.FirstOrDefault() ?? t;

                var key =
                        ContainerBuilder
                            .RegisterType(t)
                            .AsSelf()
                            .AsImplementedInterfaces()
                            .Keyed(t.FullName, keyType)
                            .UsingConstructor(new OTDConstructorSelector())
                    ;
                if (t.ImplementedInterfaces.Contains(typeof(IBinding))) // otherwise IStateBindings won't be picked up by key
                    key.Keyed<IBinding>(t.FullName);
                if (_scopedAutoloadTypes.Any(at => t.IsAssignableTo(at))) // TODO: can simplifyy any() lambda
                    key.InstancePerLifetimeScope();
            }

            ContainerBuilder.RegisterAssemblyModules(assemblies); // discover modules
        }

        public virtual IReadOnlyCollection<TypeInfo> GetChildTypes<T>()
        {
            var children = from type in PluginTypes
                           where typeof(T).IsAssignableFrom(type)
                           where !IsPluginIgnored(type)
                           select type;

            return children.ToArray();
        }

        public virtual string? GetFriendlyName(string path)
        {
            if (AppInfo.PluginManager.PluginTypes.FirstOrDefault(t => t.FullName == path) is TypeInfo plugin)
            {
                var attrs = plugin.GetCustomAttributes(true);
                var nameattr = attrs.FirstOrDefault(t => t.GetType() == typeof(PluginNameAttribute));
                if (nameattr is PluginNameAttribute attr)
                    return attr.Name;
            }
            return null;
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

        protected virtual bool IsPluginType(Type type) => type.IsPluginType();

        protected virtual bool IsPlatformSupported(Type type)
        {
            var customAttr = (SupportedPlatformAttribute?)type.GetCustomAttribute(typeof(SupportedPlatformAttribute), false);
            return customAttr?.IsCurrentPlatform ?? true;
        }

        protected virtual bool IsPluginIgnored(Type type)
        {
            return type.GetCustomAttributes(false).Any(a => a.GetType() == typeof(PluginIgnoreAttribute));
        }

        protected virtual bool IsLoadable(Assembly asm) => asm.IsLoadable();
    }
}
