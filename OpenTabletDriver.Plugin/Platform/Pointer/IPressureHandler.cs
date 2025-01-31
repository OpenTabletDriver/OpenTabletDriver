namespace OpenTabletDriver.Plugin.Platform.Pointer
{
    public interface IPressureHandler : IValidatableDevice
    {
        void SetPressure(float percentage);
    }
}
