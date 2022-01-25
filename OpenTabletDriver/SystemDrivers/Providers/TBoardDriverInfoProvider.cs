namespace OpenTabletDriver.SystemDrivers.InfoProviders
{
    internal class RobotPenDriverInfoDriver : ProcessModuleQueryableDriverInfoProvider
    {
        protected override string FriendlyName => "RobotPen";

        protected override string LinuxFriendlyName => "RobotPen";

        protected override string LinuxModuleName => "RobotPen";

        protected override string[] WinProcessNames { get; } = new string[]
        {
            "TabletDriverCenter",
            "TabletDriverSetting"
        };

        protected override string[] Heuristics { get; } = new string[]
        {
            "RobotPen"
        };
    }
}
