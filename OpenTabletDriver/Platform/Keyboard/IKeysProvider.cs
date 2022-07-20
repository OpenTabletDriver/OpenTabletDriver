using System.Collections.Generic;
using JetBrains.Annotations;

namespace OpenTabletDriver.Platform.Keyboard
{
    /// <summary>
    /// A native keys provider, mapping Eto.Forms' key names to native key types.
    /// </summary>
    [PublicAPI]
    public interface IKeysProvider
    {
        /// <summary>
        /// A dictionary mapping Eto.Forms' key names to native key types.
        /// </summary>
        IDictionary<string, object> EtoToNative { get; }
    }
}
