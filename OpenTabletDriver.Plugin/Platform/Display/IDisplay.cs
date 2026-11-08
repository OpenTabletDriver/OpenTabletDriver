using System.Numerics;

namespace OpenTabletDriver.Plugin.Platform.Display
{
    /// <summary>
    /// A display on the system
    /// </summary>
    /// <remarks>
    /// <see cref="IVirtualScreen"/> also inherits from this, so unless you're
    /// enumerating <see cref="IVirtualScreen.Displays"/> you may want to check
    /// if the implementer is also an <see cref="IVirtualScreen"/>
    /// </remarks>
    public interface IDisplay
    {
        int Index { get; }
        float Width { get; }
        float Height { get; }

        /// <summary>
        /// The position in relation to top left
        /// </summary>
        Vector2 Position { get; }

        /// <summary>
        /// The name for the display
        /// </summary>
        string? Name { get; }
    }
}
