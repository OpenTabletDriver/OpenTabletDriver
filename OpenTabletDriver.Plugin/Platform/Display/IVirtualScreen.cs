using System.Collections.Generic;

namespace OpenTabletDriver.Plugin.Platform.Display
{
    /// <summary>
    /// An <see cref="IDisplay"/> that encompasses all displays in the desktop environment.
    /// <see cref="IDisplay.Width"/> and <see cref="IDisplay.Height"/> is based on all displays.
    /// </summary>
    public interface IVirtualScreen : IDisplay
    {
        /// <summary>
        /// The individual displays contained within this virtual screen
        /// </summary>
        IEnumerable<IDisplay> Displays { get; }

        /// <summary>
        /// The name or context of the virtual screen.
        /// If you can potentially have different virtual screens, it is recommended
        /// that you override this with an appropriate string, e.g. <c>"Virtual Screen {Index}"</c>
        /// </summary>
        string IDisplay.Name => "Virtual Screen";
    }
}
