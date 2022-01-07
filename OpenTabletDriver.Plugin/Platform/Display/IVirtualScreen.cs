using System.Collections.Generic;

namespace OpenTabletDriver.Plugin.Platform.Display
{
    public interface IVirtualScreen : IDisplay
    {
        IEnumerable<IDisplay> Displays { get; }
    }
}
