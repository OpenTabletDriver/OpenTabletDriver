using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Output
{
    public interface IMouseButtonSource
    {
        IMouseButtonHandler MouseButtonHandler { get; }
    }
}
