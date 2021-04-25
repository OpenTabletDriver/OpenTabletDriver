namespace OpenTabletDriver.Plugin.Output
{
    public interface IPointerProvider<T> where T : class
    {
        T Pointer { get; }
    }
}
