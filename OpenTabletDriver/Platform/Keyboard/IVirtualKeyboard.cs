using System.Collections.Generic;
using JetBrains.Annotations;

namespace OpenTabletDriver.Platform.Keyboard
{
    /// <summary>
    /// A virtual keyboard for invoking keyboard events.
    /// </summary>
    [PublicAPI]
    public interface IVirtualKeyboard
    {
        /// <summary>
        /// Invokes a key press.
        /// </summary>
        /// <param name="key">
        /// The key to press.
        /// </param>
        void Press(BindableKey key);

        /// <summary>
        /// Invokes a key release.
        /// </summary>
        /// <param name="key">
        /// The key to release.
        /// </param>
        void Release(BindableKey key);

        /// <summary>
        /// Invokes multiple keys being pressed at once.
        /// </summary>
        /// <param name="keys">
        /// The keys to press.
        /// </param>
        void Press(IEnumerable<BindableKey> keys);

        /// <summary>
        /// Invokes multiple keys being released at once.
        /// </summary>
        /// <param name="keys">
        /// The keys to release.
        /// </param>
        void Release(IEnumerable<BindableKey> keys);

        /// <summary>
        /// A list of all supported keys in this virtual keyboard.
        /// </summary>
        IEnumerable<BindableKey> SupportedKeys { get; }
    }
}
