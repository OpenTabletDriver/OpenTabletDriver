using System.Collections.Generic;

namespace TabletDriverPlugin.Platform.Display
{
    public interface IVirtualScreen : IDisplay
    {
        IEnumerable<IDisplay> Displays { get; }
    }
}