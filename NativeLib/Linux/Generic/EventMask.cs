namespace NativeLib.Linux
{
    public enum EventMask : long
    {
        NoEventMask = 0L,
        KeyPressMask = (1L << 0),
        KeyReleaseMask = (1L << 1),
        ButtonPressMask = (1L << 2),
        ButtonReleaseMask = (1L << 3)
    }
}