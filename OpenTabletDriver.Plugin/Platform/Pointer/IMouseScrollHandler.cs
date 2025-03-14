namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IMouseScrollHandler
    {
        public void ScrollVertically(int amount);
        public void ScrollHorizontally(int amount);
    }
}
