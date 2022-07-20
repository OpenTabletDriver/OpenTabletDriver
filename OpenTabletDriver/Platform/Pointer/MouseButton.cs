using JetBrains.Annotations;

namespace OpenTabletDriver.Platform.Pointer
{
    /// <summary>
    /// A mouse button.
    /// </summary>
    [PublicAPI]
    public enum MouseButton
    {
        None = 0,
        Left = 1,
        Middle = 2,
        Right = 3,
        Backward = 4,
        Forward = 5
    }
}
