using JetBrains.Annotations;
namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// An report designating the gesture touchpad state.
    /// </summary>
    [PublicAPI]
    public interface IGestureTouchReport : IDeviceReport
    {
        bool[] TouchGestures { set; get; }
    }
}
