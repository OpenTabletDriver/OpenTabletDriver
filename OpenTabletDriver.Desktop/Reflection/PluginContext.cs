using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginContext : AssemblyLoadContext
    {
        protected static readonly string PLUGIN_ASSEMBLY_NAMESPACE = typeof(OpenTabletDriver.Plugin.Globals).Namespace ?? "OpenTabletDriver.Plugin";
        protected static readonly Assembly? PluginAssembly = Default.Assemblies.FirstOrDefault(asm => asm.GetName().FullName == PLUGIN_ASSEMBLY_NAMESPACE);

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // Reference shared dependency instead of duplicating it into this PluginContext
            if (assemblyName.Name == PLUGIN_ASSEMBLY_NAMESPACE)
            {
                // Do not load the plugin if the major & minor don't match
                if (assemblyName.Version is not { } assemblyVersion)
                    throw new NotSupportedException($"Incompatible assembly version for '{PLUGIN_ASSEMBLY_NAMESPACE}'. Found: 'null', Expected : '{AppInfo.Current.Version}' or newer");
                else if (!assemblyVersion.IsSupportedBy(AppInfo.Current.Version))
                    throw new NotSupportedException($"Incompatible assembly version for '{PLUGIN_ASSEMBLY_NAMESPACE}'. Found: '{assemblyVersion}', Expected : '{AppInfo.Current.Version}' or newer");
                else
                    return PluginAssembly;
            }

            return null;
        }
    }
}
