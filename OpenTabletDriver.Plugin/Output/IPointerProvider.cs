using System;
using JetBrains.Annotations;

namespace OpenTabletDriver.Plugin.Output
{
    // TODO: Remove type on API bump
    [PublicAPI]
    [Obsolete($"This interface provides an incomplete implementation in the 0.6.x codebase. If you're checking inheritance, use {nameof(AbsoluteOutputMode)} or {nameof(RelativeOutputMode)} instead")]
    public interface IPointerProvider<T> where T : class
    {
        T? Pointer { get; }
    }
}
