using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Reflection.Autofac
{
    public class OTDConstructorSelector : IConstructorSelector
    {
        private MostParametersConstructorSelector _autofacDefault = new();

        private readonly string _adaptiveBindingPath = "OpenTabletDriver.Desktop.Binding.AdaptiveBinding";

        public BoundConstructor SelectConstructorBinding(BoundConstructor[] constructorBindings, IEnumerable<Parameter> parameters)
        {
            var firstParam = parameters.FirstOrDefault();
            // TODO: this is slow but apparently the only way to extract the keyed parameter
            var val = firstParam?.GetType().GetDeclaredProperty("ServiceKey").GetValue(firstParam) as string;
            if (val != null && val == _adaptiveBindingPath)
            {
                Log.Debug(nameof(OTDConstructorSelector), "Intercepting constructor selection for Adaptive Binding");
                var instantiatableConstructors = constructorBindings.Where(x => x.CanInstantiate).ToArray();
                if (instantiatableConstructors.Length > 1)
                {
                    return instantiatableConstructors.First(x => // prefer IPenActionHandler first
                        x.Binder.Parameters.Any(b => b.ParameterType == typeof(IPenActionHandler)));
                }

                return instantiatableConstructors.First();
            }

            return _autofacDefault.SelectConstructorBinding(constructorBindings, parameters);
        }
    }
}
