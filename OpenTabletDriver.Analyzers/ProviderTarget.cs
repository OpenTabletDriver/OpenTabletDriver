using System;

namespace OpenTabletDriver.Analyzers
{
    public sealed class ProviderTarget : IEquatable<ProviderTarget>
    {
        public ProviderTarget(string className, string namespaceName)
        {
            ClassName = className;
            NamespaceName = namespaceName;
        }

        public string ClassName { get; }
        public string NamespaceName { get; }

        public bool Equals(ProviderTarget other)
        {
            return other != null
                && other.ClassName == ClassName
                && other.NamespaceName == NamespaceName;
        }

        public override bool Equals(object obj)
        {
            return obj is ProviderTarget target && Equals(target);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ClassName, NamespaceName);
        }
    }
}
