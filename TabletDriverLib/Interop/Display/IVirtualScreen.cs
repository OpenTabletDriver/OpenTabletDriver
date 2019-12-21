using System.Collections.Generic;

namespace TabletDriverLib.Interop.Display
{
    public interface IVirtualScreen : IDisplay
    {
        IEnumerable<IDisplay> Displays { get; }
    }
}