namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IMouseScrollHandler
    {
        void ScrollVertically(int amount);
        void ScrollHorizontally(int amount);
    }
}
