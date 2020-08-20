namespace TabletDriverPlugin.Platform.Pointer
{
    public interface IPressureHandler : IVirtualPointer
    {
        void SetPressure(float percentage);
    }
}