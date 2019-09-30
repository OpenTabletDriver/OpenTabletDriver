namespace TabletDriverLib.Interface
{
    public interface ILogger
    {
        void Write(string text);
        void WriteLine(string text);
        void WriteLine(string prefix, string text);
    }
}