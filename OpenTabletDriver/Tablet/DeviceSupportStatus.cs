using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public enum DeviceSupportStatus
    {
        Untested,
        Broken,
        Unsupported,
        MissingFeatures,
        HasQuirks,
        Supported
    }
}
