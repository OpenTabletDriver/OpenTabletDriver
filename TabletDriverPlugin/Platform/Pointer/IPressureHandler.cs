namespace TabletDriverPlugin.Platform.Pointer
{
    public interface IPressureHandler : IPointerHandler
    {
        void SetPressure(float percentage);
    }
}