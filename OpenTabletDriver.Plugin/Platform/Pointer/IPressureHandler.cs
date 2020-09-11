namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IPressureHandler : IVirtualPointer
    {
        void SetPressure(float percentage);
    }
}