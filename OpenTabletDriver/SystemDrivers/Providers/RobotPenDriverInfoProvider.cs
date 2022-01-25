namespace OpenTabletDriver.SystemDrivers.InfoProviders
{
    internal class RobotPenDriverInfoDriver : ProcessModuleQueryableDriverInfoProvider
    {
        protected override string FriendlyName => "RobotPen";

        protected override string LinuxFriendlyName => "UC Logic";

        protected override string LinuxModuleName => "hid_uclogic";

        protected override string[] WinProcessNames { get; } = new string[]
        {
            "TabletDriverCenter",
            "TabletDriverSetting"
        };

        protected override string[] Heuristics { get; } = new string[]
        {
            "RobotPen"
        };

        private string[] Exclusions = new string[]
        {
            "RobotPen"
        };
    }
}
