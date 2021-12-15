namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface ISynchronousPointer
    {
        public void Reset();
        public void Flush();
    }
}