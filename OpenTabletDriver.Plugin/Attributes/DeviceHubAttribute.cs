using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Plugin.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature, ImplicitUseTargetFlags.Itself)]
    public class DeviceHubAttribute : Attribute;
}
