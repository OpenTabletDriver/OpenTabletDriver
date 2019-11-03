namespace TabletDriverLib.Interop.Converters
{
    public interface IConverter<T1, T2>
    {
        T1 Convert(T2 obj);
        T2 Convert(T1 obj);
    }
}