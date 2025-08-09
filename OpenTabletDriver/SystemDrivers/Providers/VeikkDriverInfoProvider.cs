namespace OpenTabletDriver.SystemDrivers.InfoProviders
{
    internal class VeikkDriverInfoDriver : ProcessModuleQueryableDriverInfoProvider
    {
        protected override string FriendlyName => "Veikk";

        protected override string LinuxFriendlyName => "UC Logic";

        protected override string LinuxModuleName => "hid_uclogic";

        protected override string[] WinProcessNames { get; } = new string[]
        {
            "TabletDriverCenter",
            "TabletDriverSetting"
        };

        protected override string[] Heuristics { get; } = new string[]
        {
            "Veikk"
        };
    }
}
