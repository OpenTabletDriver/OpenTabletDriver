using System.Collections.Generic;
using JetBrains.Annotations;

namespace OpenTabletDriver.Platform.Display
{
    /// <summary>
    /// A root display server with a virtual <see cref="IDisplay"/> for the maximum usable area.
    /// </summary>
    [PublicAPI]
    public interface IVirtualScreen : IDisplay
    {
        /// <summary>
        /// An enumeration of all displays on this display server.
        /// </summary>
        IEnumerable<IDisplay> Displays { get; }
    }
}
