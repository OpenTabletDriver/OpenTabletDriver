using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace OpenTabletDriver.Platform.Keyboard
{
    /// <summary>
    /// Provides a mapping between <see cref="BindableKey"/> and platform-specific key codes.
    /// </summary>
    [PublicAPI]
    public interface IKeyMapper
    {
        /// <summary>
        /// Gets a mapping between <see cref="BindableKey"/> and platform-specific key codes.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when the specified bindable key is not supported by the platform.</exception>
        object this[BindableKey key] { get; }

        /// <summary>
        /// Gets a list of all <see cref="BindableKey"/>s that can be mapped.
        /// </summary>
        IEnumerable<BindableKey> GetBindableKeys();
    }
}
