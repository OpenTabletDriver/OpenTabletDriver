namespace OpenTabletDriver.SystemDrivers.InfoProviders
{
    internal class HuionDriverInfoProvider : ProcessModuleQueryableDriverInfoProvider
    {
        protected override string FriendlyName => "Huion";

        protected override string LinuxFriendlyName => "UC Logic";

        protected override string LinuxModuleName => "hid_uclogic";

        protected override string[] WinProcessNames { get; } = new string[]
        {
            "TabletDriverCore"
        };

        protected override string[] Heuristics { get; } = new string[]
        {
            "Huion"
        };
    }
}
