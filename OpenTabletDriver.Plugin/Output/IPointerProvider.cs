namespace OpenTabletDriver.Plugin.Output
{
    public interface IPointerProvider<out T> where T : class
    {
        T? Pointer { get; }
    }
}
