namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface ISynchronousPointer : IValidatableDevice
    {
        void Reset();
        void Flush();
    }
}
