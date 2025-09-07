using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Logging;

namespace OpenTabletDriver.Desktop
{
    public static class Extensions
    {
        /// <summary>
        /// Gets valid OpenTabletDriver plugin types from provided list of assemblies
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetPluginTypes(this IEnumerable<Assembly> assemblies)
        {
            return from asm in assemblies
                where asm.IsLoadable()
                from type in asm.GetExportedTypes()
                where IsPluginType(type)
                select type;
        }

        /// <summary>
        /// Check if provided assembly is loadable.
        /// Useful to check whether an OpenTabletDriver plugin is valid or not.
        ///
        /// Outputs via log if loading failed, unless suppressed via parameter
        /// </summary>
        /// <param name="asm">The assembly to attempt loading</param>
        /// <param name="suppressLog">Whether to suppress logging</param>
        /// <returns>Returns true if the assembly was successfully loaded, false otherwise.</returns>
        public static bool IsLoadable(this Assembly asm, bool suppressLog = false)
        {
            try
            {
                _ = asm.DefinedTypes;
                return true;
            }
            catch (Exception ex)
            {
                if (suppressLog) return false;

                var asmName = asm.GetName();
                var hResultHex = ex.HResult.ToString("X");
                var message = new LogMessage
                {
                    Group = "Plugin",
                    Level = LogLevel.Warning,
                    Message =
                        $"Plugin '{asmName.Name}, Version={asmName.Version}' can't be loaded and is likely out of date. (HResult: 0x{hResultHex})",
                    StackTrace = ex.Message + Environment.NewLine + ex.StackTrace
                };
                Log.Write(message);

                return false;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>Returns true if type is a valid OpenTabletDriver plugin, false otherwise</returns>
        public static bool IsPluginType(this Type type)
        {
            return !type.IsAbstract && !type.IsInterface &&
                   GetLibTypes().Any(t => t.IsAssignableFrom(type) ||
                                     type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == t));
        }

        private static ReadOnlyCollection<Type> libTypes;

        /// <summary>
        /// Get all abstract types and interface types implemented by IDriver
        /// </summary>
        /// <returns>An array of abstract and interface types implemented by IDriver</returns>
        public static ReadOnlyCollection<Type> GetLibTypes()
        {
            return libTypes ??=
                new ReadOnlyCollection<Type>([
                    .. from type in typeof(IDriver).Assembly.GetExportedTypes()
                    where type.IsAbstract || type.IsInterface
                    select type
                ]);
        }
    }
}
