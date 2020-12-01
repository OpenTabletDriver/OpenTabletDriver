namespace OpenTabletDriver.Plugin.Output
{
    public interface IPointerOutputMode<T> where T : class
    {
        T Pointer { get; }
    }
}
